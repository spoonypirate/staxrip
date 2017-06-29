﻿Imports System.Globalization
Imports System.Text
Imports Microsoft.Win32

<Serializable>
Public Class LogBuilder
    Private StartTime As DateTime
    Private Log As New StringBuilder

    Sub Append(content As String)
        SyncLock Log
            Log.Append(content)
        End SyncLock
    End Sub

    Sub Write(title As String, content As String)
        StartTime = DateTime.Now
        If Not ToString.EndsWith(BR2) Then Append(BR)
        Append(FormatHeader(title))

        If content <> "" Then
            If content.EndsWith(BR) Then
                Append(content)
            Else
                Append(content + BR)
            End If
        End If
    End Sub

    Sub WriteHeader(value As String)
        StartTime = DateTime.Now

        If value <> "" Then
            If Not ToString.EndsWith(BR2) Then Append(BR)
            Append(FormatHeader(value))
        End If
    End Sub

    Sub WriteLine(value As String)
        If value <> "" Then
            If value.EndsWith(BR) Then
                Append(value)
            Else
                Append(value + BR)
            End If
        End If
    End Sub

    Function FormatHeader(value As String) As String
        Return "-=".Multiply(30) + "-" + BR +
            value.PadLeft(30 + value.Length \ 2) +
            BR + "-=".Multiply(30) + "-" + BR2
    End Function

    Sub WriteEnvironment()
        If ToString.Contains("Environment" + BR + "-=") Then Exit Sub

        WriteHeader("Environment")

        Dim temp =
            "StaxRip:" + Application.ProductVersion + BR +
            "Windows:" + Registry.LocalMachine.GetString("SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName") + " " + Registry.LocalMachine.GetString("SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId") + BR +
            "Language:" + CultureInfo.CurrentCulture.EnglishName + BR +
            "CPU:" + Registry.LocalMachine.GetString("HARDWARE\DESCRIPTION\System\CentralProcessor\0", "ProcessorNameString") + BR +
            "GPU:" + String.Join(", ", SystemHelp.VideoControllers) + BR +
            "Resolution:" & Screen.PrimaryScreen.Bounds.Width & " x " & Screen.PrimaryScreen.Bounds.Height & BR +
            "DPI:" & g.MainForm.DeviceDpi

        WriteLine(temp.FormatColumn(":"))
    End Sub

    Sub WriteStats()
        WriteStats(StartTime)
    End Sub

    Sub WriteStats(start As DateTime)
        Dim n = DateTime.Now.Subtract(start)
        If Not ToString.EndsWith(BR2) Then Append(BR)
        Append("Start: ".PadRight(10) + start.ToLongTimeString + BR)
        Append("End: ".PadRight(10) + DateTime.Now.ToLongTimeString + BR)
        Append("Duration: " + CInt(Math.Floor(n.TotalHours)).ToString("d2") + ":" + n.Minutes.ToString("d2") + ":" + n.Seconds.ToString("d2") + BR)
        Append(BR)
    End Sub

    Function IsEmpty() As Boolean
        SyncLock Log
            Return Log.Length = 0
        End SyncLock
    End Function

    Public Overrides Function ToString() As String
        SyncLock Log
            Return Log.ToString
        End SyncLock
    End Function

    Sub Save(Optional proj As Project = Nothing)
        If proj Is Nothing Then proj = p

        SyncLock Log
            Log.ToString.WriteUTF8File(GetPath(proj))
        End SyncLock
    End Sub

    Function GetPath(Optional proj As Project = Nothing) As String
        If proj Is Nothing Then proj = p

        If proj.SourceFile = "" Then
            Return Folder.Temp + "staxrip.log"
        ElseIf proj.TempDir = "" Then
            Return proj.SourceFile.DirAndBase + "_staxrip.log"
        Else
            Return proj.TempDir + proj.TargetFile.Base + "_staxrip.log"
        End If
    End Function
End Class