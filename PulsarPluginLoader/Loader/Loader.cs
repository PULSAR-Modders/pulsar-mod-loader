using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace PulsarPluginLoader.Loader
{
    internal class Loader
    {
        #region Mono methods

        string mono_get_root_domain = "mono_get_root_domain";

        string mono_thread_attach = "mono_thread_attach";

        string mono_image_open_from_data = "mono_image_open_from_data";

        string mono_assembly_load_from_full = "mono_assembly_load_from_full";

        string mono_assembly_get_image = "mono_assembly_get_image";

        string mono_class_from_name = "mono_class_from_name";

        string mono_class_get_method_from_name = "mono_class_get_method_from_name";

        string mono_runtime_invoke = "mono_runtime_invoke";

        readonly Dictionary<string, IntPtr> Exports = new Dictionary<string, IntPtr>
        {
            { "mono_get_root_domain", IntPtr.Zero },
            { "mono_thread_attach", IntPtr.Zero },
            { "mono_image_open_from_data", IntPtr.Zero },
            { "mono_assembly_load_from_full", IntPtr.Zero },
            { "mono_assembly_get_image", IntPtr.Zero },
            { "mono_class_from_name", IntPtr.Zero },
            { "mono_class_get_method_from_name", IntPtr.Zero },
            { "mono_runtime_invoke", IntPtr.Zero }
        };

        #endregion

        IntPtr _handle;
        Memory _memory;
        IntPtr _mono;
        IntPtr _rootDomain;

        public static void Load(Process handle, Byte[] exeBytes, Byte[][] references) => new Loader().Load_Internal(handle, exeBytes, references);
        void Load_Internal(Process handle, Byte[] exeBytes, Byte[][] references)
        {
            _handle = Native.OpenProcess(ProcessAccessRights.PROCESS_ALL_ACCESS, false, handle.Id);
            _memory = new Memory(_handle);
            if (GetMonoModule(_handle, out _mono))
            {
                IntPtr rawImage, assembly, image, @class, method;

                ObtainMonoExports();
                _rootDomain = GetRootDomain();

                #region Load Harmony and other references
                foreach(var refAssembly in references)
                {
                    OpenAssemblyFromImage(OpenImageFromData(refAssembly));
                }
                #endregion
                #region Load PPL
                rawImage = OpenImageFromData(exeBytes);
                assembly = OpenAssemblyFromImage(rawImage);
                int timeout = 0;
                again:
                try
                {
                    image = GetImageFromAssembly(assembly);
                    @class = GetClassFromName(image, "PulsarPluginLoader.Loader", "EntryPoint");
                    method = GetMethodFromName(@class, "InitPulsarPluginLoader");
                    RuntimeInvoke(method);
                }
                catch(Exception e)
                {
                    if(timeout < 3)
                    {
                        timeout++;
                        goto again;
                    }
                    else
                    {
                        Console.WriteLine(e);
                        Console.WriteLine("Press any key...");
                        Console.ReadKey();
                        Environment.Exit(-1);
                    }
                }
                #endregion
            }
            else Console.WriteLine("Error in GetMonoModule");
            _memory.Dispose();
        }
        void ObtainMonoExports()
        {
            foreach (ExportedFunction ef in GetExportedFunctions(_handle, _mono))
                if (Exports.ContainsKey(ef.Name))
                    Exports[ef.Name] = ef.Address;

            foreach (var kvp in Exports)
                if (kvp.Value == IntPtr.Zero)
                    throw new Exception($"Failed to obtain the address of {kvp.Key}()");
        }

        bool GetMonoModule(IntPtr handle, out IntPtr monoModule)
        {
            int size = 8;

            IntPtr[] ptrs = new IntPtr[0];

            if (!Native.EnumProcessModulesEx(handle, ptrs, 0, out int bytesNeeded, ModuleFilter.LIST_MODULES_ALL))
            {
                throw new Exception("Failed to enumerate process modules", new Win32Exception(Marshal.GetLastWin32Error()));
            }

            int count = bytesNeeded / size;
            ptrs = new IntPtr[count];

            if (!Native.EnumProcessModulesEx(handle, ptrs, bytesNeeded, out bytesNeeded, ModuleFilter.LIST_MODULES_ALL))
            {
                throw new Exception("Failed to enumerate process modules", new Win32Exception(Marshal.GetLastWin32Error()));
            }

            for (int i = 0; i < count; i++)
            {
                StringBuilder path = new StringBuilder(260);
                Native.GetModuleFileNameEx(handle, ptrs[i], path, 260);

                if (path.ToString().IndexOf("mono", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    if (!Native.GetModuleInformation(handle, ptrs[i], out MODULEINFO info, (uint)(size * ptrs.Length)))
                        throw new Exception("Failed to get module information", new Win32Exception(Marshal.GetLastWin32Error()));

                    var funcs = GetExportedFunctions(handle, info.lpBaseOfDll);

                    if (funcs.Any(f => f.Name == "mono_get_root_domain"))
                    {
                        monoModule = info.lpBaseOfDll;
                        return true;
                    }
                }
            }

            monoModule = IntPtr.Zero;
            return false;
        }

        IEnumerable<ExportedFunction> GetExportedFunctions(IntPtr handle, IntPtr mod)
        {
            using (Memory memory = new Memory(handle))
            {
                int e_lfanew = memory.ReadInt(mod + 0x3C);
                IntPtr ntHeaders = mod + e_lfanew;
                IntPtr optionalHeader = ntHeaders + 0x18;
                IntPtr dataDirectory = optionalHeader + 0x70;
                IntPtr exportDirectory = mod + memory.ReadInt(dataDirectory);
                IntPtr names = mod + memory.ReadInt(exportDirectory + 0x20);
                IntPtr ordinals = mod + memory.ReadInt(exportDirectory + 0x24);
                IntPtr functions = mod + memory.ReadInt(exportDirectory + 0x1C);
                int count = memory.ReadInt(exportDirectory + 0x18);

                for (int i = 0; i < count; i++)
                {
                    int offset = memory.ReadInt(names + i * 4);
                    string name = memory.ReadString(mod + offset, 32, Encoding.ASCII);
                    short ordinal = memory.ReadShort(ordinals + i * 2);
                    IntPtr address = mod + memory.ReadInt(functions + ordinal * 4);

                    if (address != IntPtr.Zero)
                        yield return new ExportedFunction(name, address);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ThrowIfNull(IntPtr ptr, string methodName)
        {
            if (ptr == IntPtr.Zero)
                throw new Exception($"{methodName}() returned NULL");
        }

        IntPtr GetRootDomain()
        {
            IntPtr rootDomain = ExecFunc(Exports[mono_get_root_domain]); // For some reason, the original Execute returns null.
            ThrowIfNull(rootDomain, mono_get_root_domain);
            return rootDomain;
        }

        IntPtr ExecFunc(IntPtr funcAddr, params IntPtr[] args)
        {
            var asm = new List<Byte>();
            var retVal = Native.VirtualAllocEx(_handle, IntPtr.Zero, 8, (AllocationType)0x3000, (MemoryProtection)4);
            Native.WriteProcessMemory(_handle, retVal, BitConverter.GetBytes(0xdeadbeefcafef00d), 8);
            asm.AddRange(new Byte[] { 0x48, 0x83, 0xEC, 0x38 }); // sub rsp 0x38
            for (var i = 0; i < args.Length && i < 4; i++)
            {
                if (i == 0) asm.AddRange(new Byte[] { 0x48, 0xB9 }); // mov rcx
                if (i == 1) asm.AddRange(new Byte[] { 0x48, 0xBA }); // mov rdx
                if (i == 2) asm.AddRange(new Byte[] { 0x49, 0xB8 }); // mov r8
                if (i == 3) asm.AddRange(new Byte[] { 0x49, 0xB9 }); // mov r9
                asm.AddRange(BitConverter.GetBytes((UInt64)args[i]));
            }
            asm.AddRange(new Byte[] { 0x48, 0xB8 }); // mov rax
            asm.AddRange(BitConverter.GetBytes((UInt64)funcAddr));

            asm.AddRange(new Byte[] { 0xFF, 0xD0 }); // call rax
            asm.AddRange(new Byte[] { 0x48, 0x83, 0xC4, 0x38 }); // add rsp 0x38

            asm.AddRange(new Byte[] { 0x48, 0xA3 }); // mov rax to retval
            asm.AddRange(BitConverter.GetBytes((UInt64)retVal));
            asm.AddRange(Enumerable.Range(0, 0x20).Select(a => (byte)0x90));
            asm.Add(0xC3); // ret
            var codePtr = Native.VirtualAllocEx(_handle, IntPtr.Zero, asm.Count, (AllocationType)0x3000, (MemoryProtection)0x40);
            Native.WriteProcessMemory(_handle, codePtr, asm.ToArray(), asm.Count);
            var qq = BitConverter.ToString(asm.ToArray()).Replace("-", " ");
            var thread = Native.CreateRemoteThread(_handle, IntPtr.Zero, 0, codePtr, IntPtr.Zero, 0, out _);
            Native.WaitForSingleObject(thread, 10000);
            var buf = new Byte[8u];
            Native.ReadProcessMemory(_handle, retVal, buf, buf.Length);
            Native.VirtualFreeEx(_handle, retVal, 0, (MemoryFreeType)0x8000);
            Native.VirtualFreeEx(_handle, codePtr, 0, (MemoryFreeType)0x8000);
            Native.CloseHandle(thread);
            return (IntPtr)(BitConverter.ToInt64(buf, 0));
        }
        IntPtr OpenImageFromData(byte[] assembly)
        {
            IntPtr statusPtr = _memory.Allocate(4);
            IntPtr rawImage = Execute(Exports[mono_image_open_from_data], _memory.AllocateAndWrite(assembly), (IntPtr)assembly.Length, (IntPtr)1, statusPtr);

            return rawImage;
        }

        IntPtr OpenAssemblyFromImage(IntPtr image)
        {
            IntPtr statusPtr = _memory.Allocate(4);
            IntPtr assembly = Execute(Exports[mono_assembly_load_from_full], image, _memory.AllocateAndWrite(new byte[1]), statusPtr, IntPtr.Zero);

            return assembly;
        }

        IntPtr GetImageFromAssembly(IntPtr assembly)
        {
            IntPtr image = Execute(Exports[mono_assembly_get_image], assembly);
            ThrowIfNull(image, mono_assembly_get_image);
            return image;
        }

        IntPtr GetClassFromName(IntPtr image, string @namespace, string className)
        {
            IntPtr @class = Execute(Exports[mono_class_from_name], image, _memory.AllocateAndWrite(@namespace), _memory.AllocateAndWrite(className));
            //IntPtr @class = ExecFunc(Exports[mono_class_from_name], image, _memory.AllocateAndWrite(@namespace), _memory.AllocateAndWrite(className));
            ThrowIfNull(@class, mono_class_from_name);
            return @class;
        }

        IntPtr GetMethodFromName(IntPtr @class, string methodName)
        {
            IntPtr method = Execute(Exports[mono_class_get_method_from_name], @class, _memory.AllocateAndWrite(methodName), IntPtr.Zero);
            ThrowIfNull(method, mono_class_get_method_from_name);
            return method;
        }

        void RuntimeInvoke(IntPtr method)
        {
            Execute(Exports[mono_runtime_invoke], method, IntPtr.Zero, IntPtr.Zero, _memory.AllocateAndWrite((long)0));
        }

        IntPtr Execute(IntPtr address, params IntPtr[] args)
        {
            IntPtr retValPtr = _memory.AllocateAndWrite((long)0);

            byte[] code = Assemble64(address, retValPtr, args);
            IntPtr alloc = _memory.AllocateAndWrite(code);

            IntPtr thread = Native.CreateRemoteThread(_handle, IntPtr.Zero, 0, alloc, IntPtr.Zero, 0, out _);

            Native.WaitForSingleObject(thread, -1);

            IntPtr ret = (IntPtr)_memory.ReadLong(retValPtr);

            return ret;
        }

        byte[] Assemble64(IntPtr functionPtr, IntPtr retValPtr, IntPtr[] args)
        {
            Assembler asm = new Assembler();

            asm.SubRsp(40);

            asm.MovRax(Exports[mono_thread_attach]);
            asm.MovRcx(_rootDomain);
            asm.CallRax();

            asm.MovRax(functionPtr);

            for (int i = 0; i < args.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        asm.MovRcx(args[i]);
                        break;
                    case 1:
                        asm.MovRdx(args[i]);
                        break;
                    case 2:
                        asm.MovR8(args[i]);
                        break;
                    case 3:
                        asm.MovR9(args[i]);
                        break;
                }
            }

            asm.CallRax();
            asm.AddRsp(40);
            asm.MovRaxTo(retValPtr);
            asm.Return();

            return asm.ToByteArray();
        }

        public struct ExportedFunction
        {
            public string Name;

            public IntPtr Address;

            public ExportedFunction(string name, IntPtr address)
            {
                Name = name;
                Address = address;
            }
        }
    }
}
