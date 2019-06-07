Imports ZSDK_API.Discovery
Imports System.Threading
Imports ZSDK_API.ApiException
Imports ZSDK_API.Comm
Imports ZSDK_API.Printer
Imports ZSDK_API
Imports System.Text
Imports System.ComponentModel

Public Class Form1
    Private ipAddress As String = "192.168.1.250"
    Private port As Integer = 9100
    Private printer As ZebraPrinter
    Private printerConnexion As ZebraPrinterConnection
    Private texte As String

    ' 1 Première initialisation
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        updateGuiFromWorkerThread("Discovery in progress...", Color.HotPink)
        Dim t As New Thread(AddressOf doDiscovery)
        t.Start()
    End Sub

    Private Sub makeLabel()
        texte = String.Format(
            TextBox1.Text,
            Strings.Left(designation.Text, 50),
            refMag.Text,
            colisage.Text,
            emplacement.Text,
            refFourn.Text,
            gencodeFourn.Text,
            prix.Text,
            gencodeMag.Text,
            conditionnement.Text)
        TextBox2.Text = texte
    End Sub

    ' Imprimante connectée, on lance l'impression d'une etiquette
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try
            makeLabel
            printerConnexion.Write(Encoding.Default.GetBytes(texte))
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub PrintLabel()
        Debug.Print("Wait For thread finishe")
        doConnectTcp()
        Debug.Print("print")
    End Sub


    ' 2 RECHERCHE DE L'IMPRIMANTE '
    Private Sub doDiscovery()

        ' On choisi le multicast par defaut
        Try
            Dim printers() As DiscoveredPrinter = NetworkDiscoverer.Multicast(5)

            If printers.Count > 0 Then
                ipAddress = printers(0).Address
                Dim t As New Thread(AddressOf doConnectTcp)
                t.Start()
            Else
                updateGuiFromWorkerThread("Imprimante non trouvée", Color.Red)
            End If

        Catch ex As DiscoveryException
            handleException(ex.Message)
        End Try
    End Sub

    ' 3 Création de la connexion TCP
    Private Sub doConnectTcp()
        updateGuiFromWorkerThread("Connecting... Please wait...", Color.Goldenrod)
        Try
            printerConnexion = New TcpPrinterConnection(Me.ipAddress, Me.port)
            threadedConnect()
            'sendLabel()
            'disconnect()
        Catch generatedExceptionName As ZebraException
            updateGuiFromWorkerThread("COMM Error! Disconnected", Color.Red)
            doDisconnect()
        End Try
    End Sub

    ' 4 Ouverture de la connexion
    Private Sub threadedConnect()
        'Thread.Sleep(10000)
        Try
            printerConnexion.Open()
        Catch generatedExceptionName As ZebraPrinterConnectionException
            updateGuiFromWorkerThread("Unable to connect with printer", Color.Red)
            disconnectPrinter()
        Catch generatedExceptionName As Exception
            updateGuiFromWorkerThread("Error communicating with printer", Color.Red)
            disconnectPrinter()
        End Try
        printer = Nothing
        If printerConnexion IsNot Nothing AndAlso printerConnexion.IsConnected() Then
            Try
                printer = ZebraPrinterFactory.GetInstance(printerConnexion)

                Dim pl As PrinterLanguage = printer.GetPrinterControlLanguage
                updateGuiFromWorkerThread(("Printer Language " + pl.ToString), Color.LemonChiffon)
                updateGuiFromWorkerThread(("Imprimante connectée"), Color.YellowGreen)

            Catch generatedExceptionName As ZebraPrinterConnectionException
                updateGuiFromWorkerThread("Unknown Printer Language", Color.Red)
                printer = Nothing
                disconnectPrinter()
            Catch generatedExceptionName As ZebraPrinterLanguageUnknownException
                updateGuiFromWorkerThread("Unknown Printer Language", Color.Red)
                printer = Nothing
                disconnectPrinter()
            End Try
        End If
    End Sub

    Public Sub disconnectPrinter()
        Dim t As New Thread(AddressOf doDisconnect)
        t.Start()
    End Sub

    Private Sub doDisconnect()
        Try
            If printerConnexion IsNot Nothing AndAlso printerConnexion.IsConnected() Then
                updateGuiFromWorkerThread("Disconnecting...", Color.Honeydew)

                printerConnexion.Close()
            End If
        Catch generatedExceptionName As ZebraException
            updateGuiFromWorkerThread("COMM Error! Disconnected", Color.Red)
        End Try
        updateGuiFromWorkerThread("Not Connected", Color.Red)
        printerConnexion = Nothing
        'updateButtonFromWorkerThread(True)
    End Sub

    Public Function getPrinter() As ZebraPrinter
        Return printer
    End Function

    Public Function getPrinterConnection() As ZebraPrinterConnection
        Return printerConnexion
    End Function
    ' The following methods update the discovered printers listbox
    Private Sub handleException(ByVal message As [String])
        updateGuiFromWorkerThread(message, Color.Red)
    End Sub
    Private Delegate Sub MyProgressEventsHandler(ByVal sender As Object, ByVal printers As DiscoveredPrinter())


    ' The following methods update the status bar at top of screen
    Public Sub updateGuiFromWorkerThread(ByVal message As [String], ByVal color As Color)
        Invoke(New StatusEventHandler(AddressOf UpdateUI), New StatusArguments(message, color))
    End Sub

    'Delegate for the status event handler
    Private Delegate Sub StatusEventHandler(ByVal e As StatusArguments)

    ' The following method updates the status bar
    Private Sub UpdateUI(ByVal e As StatusArguments)
        statusBar.Text = e.message
        statusBar.BackColor = e.color
    End Sub

    ' Status Bar data class - holds the text to be displayed and the background color of the label
    Private Class StatusArguments
        Inherits System.EventArgs
        Public message As [String]
        Public color As Color
        Public Sub New(ByVal message As [String], ByVal color As Color)
            Me.message = message
            Me.color = color
        End Sub
    End Class

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        makeLabel
    End Sub
End Class
