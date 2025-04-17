using System.Runtime.InteropServices;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.XR;
public class WindowSetting : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

     [DllImport("dwmapi.dll")]
    public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowLong(IntPtr hwnd, int _nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy,
        uint uFlags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

    [DllImport("user32.dll")]
    private static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
    public enum DWMWINDOWATTRIBUTE
    {
        DWMWA_WINDOW_CORNER_PREFERENCE = 33
    }

    public enum DWM_WINDOW_CORNER_PREFERENCE
    {
        DWMWCP_DEFAULT = 0,
        DWMWCP_DONOTROUND = 1,
        DWMWCP_ROUND = 2,
        DWMWCP_ROUNDSMALL = 3
    }
    private struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public int showCmd;
        public System.Drawing.Point ptMinPosition;
        public System.Drawing.Point ptMaxPosition;
        public System.Drawing.Rectangle rcNormalPosition;
    }

    const int SW_SHOWMINIMIZED = 2; //{��С��, ����}
    const int SW_SHOWMAXIMIZED = 3; //���
    const int SW_SHOWRESTORE = 1; //��ԭ
    const uint SWP_SHOWWINDOW = 0x0040;
    const int GWL_STYLE = -16;
    const int WS_POPUP = 0x800000;
    private Rect _screenPosition;
    private static IntPtr _handle;

    public betterButton Header;

    /// <summary>
    /// �Զ�����Ļ�ֱ��� ����
    /// </summary>
    private int _screenWidth, _screenHeight;

    private void Start()
    {
        _handle = FindWindow(null, Application.productName);

        Header.OnClickdown.AddListener(() =>
        {
            DragWindowsMethod();
        });
        //�Զ��崰�ڷֱ���
        _screenWidth = 1054;
        _screenHeight = 750;
        Resolution[] r = Screen.resolutions;
        _screenPosition =
new Rect((r[r.Length - 1].width - Screen.width) / 2, (r[r.Length - 1].height - Screen.height) / 2, _screenWidth, _screenHeight);
        SetNoFrameWindowWithRoundCorners(_screenPosition);
    }

    /// <summary>
    /// ��С�� ����
    /// </summary>
    public static void OnClickMinimize()
    {
        ShowWindow(GetForegroundWindow(), SW_SHOWMINIMIZED);
    }

    public static bool GetPlacement()
    {
        WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
        placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));

        if (GetWindowPlacement(_handle, ref placement))
        {
            if (placement.showCmd == SW_SHOWMAXIMIZED)
            {
                return true;
            }
            else if (placement.showCmd == SW_SHOWRESTORE)
            {
                return false;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    /// <summary>
    /// ��� ȫ��
    /// </summary>
    public static void OnClickimize()
    {
        if (GetPlacement())
        {
            ShowWindow(GetForegroundWindow(), SW_SHOWRESTORE);
        }
        else
        {
            ShowWindow(GetForegroundWindow(), SW_SHOWMAXIMIZED);
        }
    }
    public void SetNoFrameWindowWithRoundCorners(Rect rect)
    {
#if !UNITY_EDITOR
        // �������ޱ߿�
        SetWindowLong(_handle, GWL_STYLE, WS_POPUP);

        // ���ô���λ�úʹ�С
        bool result = SetWindowPos(_handle, 0, (int)rect.x, (int)rect.y,
                                 (int)rect.width, (int)rect.height, SWP_SHOWWINDOW);

        // ����Բ��
        int cornerPreference = (int)DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
        DwmSetWindowAttribute(_handle,
                             (int)DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE,
                             ref cornerPreference, sizeof(int));
#endif
    }

    /// <summary>
    /// �˳�
    /// </summary>
    public void CloseWindow()
    {
        Application.Quit();
    }

    /// <summary>
    /// ������ק����  �˴����ÿ����ڰ�ť�����EventTrigger��� ʹ��Drag����
    /// </summary>
    public void DragWindowsMethod()
    {
        ReleaseCapture();
        SendMessage(_handle, 0xA1, 0x02, 0);
        SendMessage(_handle, 0x0202, 0, 0);
    }
}
