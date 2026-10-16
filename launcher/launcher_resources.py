"""Add a Windows string-table name used by the taskbar relaunch property."""
import ctypes,struct
from ctypes import wintypes

def add_taskbar_name(path):
    kernel=ctypes.WinDLL('kernel32',use_last_error=True)
    kernel.BeginUpdateResourceW.argtypes=[wintypes.LPCWSTR,wintypes.BOOL];kernel.BeginUpdateResourceW.restype=wintypes.HANDLE
    kernel.UpdateResourceW.argtypes=[wintypes.HANDLE,wintypes.LPCWSTR,wintypes.LPCWSTR,wintypes.WORD,wintypes.LPVOID,wintypes.DWORD];kernel.UpdateResourceW.restype=wintypes.BOOL
    kernel.EndUpdateResourceW.argtypes=[wintypes.HANDLE,wintypes.BOOL];kernel.EndUpdateResourceW.restype=wintypes.BOOL
    # RT_STRING block 1, string ID 1. The remaining fifteen entries are empty.
    text='Synapse X'.encode('utf-16le');data=struct.pack('<HH',0,len(text)//2)+text+bytes(14*2)
    handle=kernel.BeginUpdateResourceW(str(path),False)
    if not handle:raise ctypes.WinError(ctypes.get_last_error())
    buffer=ctypes.create_string_buffer(data)
    try:
        if not kernel.UpdateResourceW(handle,ctypes.cast(ctypes.c_void_p(6),wintypes.LPCWSTR),ctypes.cast(ctypes.c_void_p(1),wintypes.LPCWSTR),0,buffer,len(data)):raise ctypes.WinError(ctypes.get_last_error())
    except BaseException:kernel.EndUpdateResourceW(handle,True);raise
    if not kernel.EndUpdateResourceW(handle,False):raise ctypes.WinError(ctypes.get_last_error())
