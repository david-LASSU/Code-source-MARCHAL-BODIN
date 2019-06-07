Imports System.Text
Imports ZSDK_API.Comm
Imports ZSDK_API.Printer

Module Module1
    Private printer As ZebraPrinter
    Private printerConnexion As ZebraPrinterConnection
    Private ipAddress As String = "192.168.1.83"
    Private port As Integer = 2001

    Sub Main()

        'Dim str As String = "^XA^FO30,70^FR^A0N,60,40^FDFIN^FS^XZ"
        Dim str As String = "^XA^FO17,16^GB379,371,8^FS^FT65,255^A0N,135,134^FDTEST^FS^XZ"
        'Dim str As String = "A50,0,0,1,1,1,N,""DATA"""
        'Dim str As String = "^XA^JUF^XZ"

        'printerConnexion = New TcpPrinterConnection(ipAddress, port)
        'Try
        '    printerConnexion.Open()

        '    If printerConnexion IsNot Nothing AndAlso printerConnexion.IsConnected
        '        printer = ZebraPrinterFactory.GetInstance(printerConnexion)
        '        Dim pl As PrinterLanguage = printer.GetPrinterControlLanguage
        '        'Debug.Print(printer.GetCurrentStatus.IsReadyToPrint)
        '        Debug.Print(pl.ToString)

        '        printerConnexion.Write(Encoding.Default.GetBytes(str))

        '        printerConnexion.Close
        '    End If

        'Catch ex As Exception
        '    Debug.Print(ex.Message)
        'End Try


        Dim ZPLString As String =
              "^XA" &
              "^FO50,50" &
              "^A0N,50,50" &
              "^FDHello, World!^FS" &
              "^XZ"

        Dim EplString As String = ""

        Try
              'Open Connection
              Dim client As New System.Net.Sockets.TcpClient
              client.Connect(ipAddress, port)
 
              'Write ZPL String to Connection
              Dim writer As New System.IO.StreamWriter(client.GetStream())
              writer.Write(Encoding.Default.GetBytes(ZPLString))
              writer.Flush()
 
              'Close Connection
              writer.Close()
              client.Close()
 
        Catch ex As Exception
 
              'Catch Exception Here
            Debug.Print(ex.Message)
        End Try
    End Sub

End Module
