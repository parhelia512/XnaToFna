﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace XnaToFna.Forms {
    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
    public struct ProxyMessage {
        
        public IntPtr HWnd {
            get; set;
        }

        public int Msg {
            get; set;
        }

        public IntPtr WParam {
            get; set;
        }

        public IntPtr LParam {
            get; set;
        }

        public IntPtr Result {
            get; set;
        }

        public object GetLParam(Type cls)
            => Marshal.PtrToStructure(LParam, cls);

        public static ProxyMessage Create(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
            => new ProxyMessage {
                HWnd = hWnd,
                Msg = msg,
                WParam = wparam,
                LParam = lparam,
                Result = IntPtr.Zero
            };

        public override bool Equals(object o) {
            if (!(o is ProxyMessage))
                return false;
            ProxyMessage other = (ProxyMessage) o;
            return
                HWnd == other.HWnd &&
                Msg == other.Msg &&
                WParam == other.WParam &&
                LParam == other.LParam &&
                Result == other.Result;
        }

        public static bool operator !=(ProxyMessage a, ProxyMessage b)
            => !a.Equals(b);

        public static bool operator ==(ProxyMessage a, ProxyMessage b)
            => a.Equals(b);

        public override int GetHashCode()
            => (int) HWnd << 4 | Msg;

        public override string ToString() {
            bool unrestricted = false;
            try {
                ProxyMessageHelper.UnmanagedCode.Demand();
                unrestricted  = true;
            } catch (SecurityException) {
            }
            return unrestricted ?
                ProxyMessageHelper.ToString(this) :
                base.ToString();
        }
    }

    public static class ProxyMessageHelper {

        public readonly static CodeAccessPermission UnmanagedCode = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);

        public static string MsgToString(int msg) {
            if (Enum.IsDefined(typeof(ProxyMessages), msg))
                return Enum.GetName(typeof(ProxyMessages), msg);

            if (((msg & (int) ProxyMessages.WM_REFLECT) == (int) ProxyMessages.WM_REFLECT)) {
                string subtext = MsgToString(msg & (int) ~ProxyMessages.WM_REFLECT);
                if (subtext == null) subtext = "???";
                return "WM_REFLECT + " + subtext;
            }

            return null;
        }

        public static StringBuilder Parenthesize(this StringBuilder builder, string input)
            => !string.IsNullOrEmpty(input) ? builder.Append(" (").Append(input).Append(")") : builder;


        public static string ToString(ProxyMessage message) {
            return ToString(message.HWnd, message.Msg, message.WParam, message.LParam, message.Result);
        }

        public static string ToString(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam, IntPtr result) {
            StringBuilder builder = new StringBuilder();
            builder
                .Append("msg=0x").Append(Convert.ToString(msg, 16)).Parenthesize(MsgToString(msg))
                .Append(" hwnd=0x").Append(Convert.ToString((long) hWnd, 16))
                .Append(" wparam=0x").Append(Convert.ToString((long) wparam, 16))
                .Append(" lparam=0x").Append(Convert.ToString((long) lparam, 16))
                .Parenthesize(msg == (int) ProxyMessages.WM_PARENTNOTIFY ? MsgToString((int) wparam & 0x0000FFFF) : null)
                .Append(" result=0x").Append(Convert.ToString((long) result, 16));
            return builder.ToString();
        }

    }

}
