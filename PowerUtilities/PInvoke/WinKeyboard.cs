using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;

internal static class NativeMethods
{

    /// <summary>
    /// 
    /// GetKeyboardState
    /// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getkeyboardstate
    /// 256 byte array
    /// keycode : https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
    /// 
    /// </summary>
    /// <param name="lpKeyState"></param>
    /// <returns></returns>
    [DllImport("User32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetKeyboardState(byte[] lpKeyState);

    [DllImport("User32.dll")]
    internal static extern short GetAsyncKeyState(int vkey);
}

public static class WinKeyboard
{
    static byte[] lastKeys = new byte[256];
    const byte KEY_DOWN = 0x80;
    const byte KEY_LOWER_HOLD = 1;

    static List<byte> downKeys = new List<byte>();
    static List<byte> upKeys = new List<byte>();

    public static void UpdateKeyboardState()
    {
        downKeys.Clear();
        upKeys.Clear(); 

        var keys = new byte[256];
        if(! NativeMethods.GetKeyboardState(keys))
        {
        }
        for (int i = 0; i < 256; i++)
        {
            if (lastKeys[i] != keys[i])
            {
                if ((keys[i] & KEY_DOWN) > 0)
                {
                    downKeys.Add((byte)i);
                }
                else
                {
                    upKeys.Add((byte)i);
                }
            }
        }
        lastKeys = keys;
    }

    public static bool IsKeyDown(byte keyIndex)
    {
        return downKeys.Contains(keyIndex);
    }
    public static bool IsKeyUp(byte keyIndex)
    {
        return upKeys.Contains(keyIndex);
    }
    public static bool IsKeyHolding(byte keyIndex)
    {
        return (lastKeys[keyIndex] & KEY_DOWN) > 0;
    }
    
    public static bool IsKeyDown(string keyCode)
    {
        var id = Array.FindIndex(KeyCodeStrings,k=> k == keyCode);
        Assert.AreNotEqual(id, -1,$"{keyCode} not found");
        return IsKeyDown((byte)id);
    }
    public static bool IsKeyUp(string keyCode)
    {
        var id = Array.FindIndex(KeyCodeStrings, k => k == keyCode);
        Assert.AreNotEqual(id, -1, $"{keyCode} not found");
        return IsKeyUp((byte)id);
    }

    public static bool IsKeyHolding(string keyCode)
    {
        var id = Array.FindIndex(KeyCodeStrings, k => k == keyCode);
        Assert.AreNotEqual(id, -1, $"{keyCode} not found");
        return IsKeyHolding((byte)id);
    }

    public static bool IsKeyHoldingAsync(string keyCode)
    {
        var id = Array.FindIndex(KeyCodeStrings, k => k == keyCode);
        Assert.AreNotEqual(id, -1, $"{keyCode} not found");
        return (NativeMethods.GetAsyncKeyState(id) & 0x01) > 0;
    }

    #region keycodes
    public static readonly string[] KeyCodeStrings = new[] {
        "",
        "Left mouse button",
        "right mouse button",
        "Control-break processing",
        "Middle mouse button",
        "X1 mouse button",
        "X2 mouse button",
        "Undefined",
        "BackSpace",
        "Tab",
        "Reserved",
        "Reserved",
        "Clear",
        "Enter",
        "Undefined",
        "Undefined",
        "Shift",
        "Ctrl",
        "Alt",
        "Pause",
        "CapsLock",
        "IME Kana mode",
        "IME On",
        "IME Junja",
        "IME final mode",
        "IME Kanji mode",
        "IME Off",
        "Esc",
        "IME convert",
        "IME nonconvert",
        "IME accept",
        "IME mode change request",
        "SpaceBar",
        "Page Up",
        "Page Down",
        "End",
        "Home",
        "←",
        "↑",
        "→",
        "↓",
        "Select",
        "Print",
        "Execute",
        "PrintScreen",
        "Insert",
        "Delete",
        "Help",
        "0",
        "1",
        "2",
        "3",
        "4",
        "5",
        "6",
        "7",
        "8",
        "9",
        "Undefined",
        "Undefined",
        "Undefined",
        "Undefined",
        "Undefined",
        "Undefined",
        "Undefined",
        "A",
        "B",
        "C",
        "D",
        "E",
        "F",
        "G",
        "H",
        "I",
        "J",
        "K",
        "L",
        "M",
        "N",
        "O",
        "P",
        "Q",
        "R",
        "S",
        "T",
        "U",
        "V",
        "W",
        "X",
        "Y",
        "Z",
        "Left Windows",
        "Right Windows",
        "Applications key",
        "Reserved",
        "Sleep",
        "Numeric Keypad 0",
        "Numeric Keypad 1",
        "Numeric Keypad 2",
        "Numeric Keypad 3",
        "Numeric Keypad 4",
        "Numeric Keypad 5",
        "Numeric Keypad 6",
        "Numeric Keypad 7",
        "Numeric Keypad 8",
        "Numeric Keypad 9",
        "Numeric Keypad *",
        "Numeric Keypad +",
        "Separator key",
        "Numeric Keypad -",
        "Numeric Keypad .",
        "Numeric Keypad /",
        "F1",
        "F2",
        "F3",
        "F4",
        "F5",
        "F6",
        "F7",
        "F8",
        "F9",
        "F10",
        "F11",
        "F12",
        "F13",
        "F14",
        "F15",
        "F16",
        "F17",
        "F18",
        "F19",
        "F20",
        "F21",
        "F22",
        "F23",
        "F24",
        "Unassigned",
        "Unassigned",
        "Unassigned",
        "Unassigned",
        "Unassigned",
        "Unassigned",
        "Unassigned",
        "Unassigned",
        "NumLock",
        "ScrollLock",
        "OEM specific",
        "OEM specific",
        "OEM specific",
        "OEM specific",
        "OEM specific",
        "Unassigned",
        "Unassigned",
        "Unassigned",
        "Unassigned",
        "Unassigned",
        "Unassigned",
        "Unassigned",
        "Unassigned",
        "Unassigned",
        "Left Shift",
        "Right Shift",
        "Left Ctrl",
        "Right Ctrl",
        "Left Alt",
        "Right Alt",
        "Browser Back key",
        "Browser Forward key",
        "Browser Refresh key",
        "Browser Stop key",
        "Browser Search key",
        "Browser Favorites key",
        "Browser Start and Home key",
        "Volume Mute key",
        "Volume Down key",
        "Volume Up key",
        "Next Track key",
        "Previous Track key",
        "Stop Media key",
        "Play/Pause Media key",
        "Start Mail key",
        "Select Media key",
        "Start Application 1 key",
        "Start Application 2 key",
        "Reserved",
        "Reserved",
        "[:*]",
        "[;+]",
        "[,<]",
        "[-=]",
        "[.>]",
        "[/?]",
        "[@`]",
        "Reserved",
        "Reserved",
        "Reserved",
        "Reserved",
        "Reserved",
        "Reserved",
        "Reserved",
        "Reserved",
        "Reserved",
        "Reserved",
        "Reserved",
        "Reserved",
        "Reserved",
        "Reserved",
        "Reserved",
        "Reserved",
        "Reserved",
        "Reserved",
        "Reserved",
        "Reserved",
        "Reserved",
        "Reserved",
        "Reserved",
        "Unassigned",
        "Unassigned",
        "Unassigned",
        "[[{]",
        "[\\|]",
        "[]}]",
        "[^~]",
        "OEM8",
        "Reserved",
        "OEM specific",
        "[＼_]",
        "OEM specific",
        "OEM specific",
        "IME PROCESS",
        "OEM specific",
        "仮想キー下位ワード",
        "Unassigned",
        "OEM specific",
        "OEM specific",
        "OEM specific",
        "OEM specific",
        "OEM specific",
        "OEM specific",
        "OEM specific",
        "英数",
        "OEM specific",
        "OEM specific",
        "OEM specific",
        "OEM specific",
        "OEM specific",
        "Attn",
        "CrSel",
        "ExSel",
        "Erase EOF",
        "Play",
        "Zoom",
        "Reserved",
        "PA1",
        "Clear"
    };
    #endregion
}

