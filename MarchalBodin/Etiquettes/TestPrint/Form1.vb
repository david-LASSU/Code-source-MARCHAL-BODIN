Imports System.Drawing.Printing
Imports Etiquettes
Imports Zen.Barcode
Imports System.Linq

Public Class Form1

    Private pLabels As New List(Of PrintLabel)
    Private LabelsToPrint As ArrayList

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim formats = New FormatCollection()

        Dim format As Format = (From f As Format In formats Where f.key = FormatNormal.FORMAT_3 Select f).First
        For index = 1 To 15
            pLabels.Add(New PrintLabel With {
                .format = format,
                .designation = Strings.Left("MESURE POWERLOCK 3M AVEC DESIGNATION SUR DEUX LIGNES", 50),
                .refMag = "1001-6666",
                .colisage = 100,
                .emplacement = "B10 E10 R10 N10",
                .gencodeFourn = "370009223482",
                .gencodeMag = "",
                .refFourn = "0-33-2389999999F",
                .prix = "1300,10",
                .count = 1})
        Next

        'pLabels.Add(New PrintLabel With {
        '    .format = format,
        '    .designation = Strings.Left("MESURE POWERLOCK 10M AVEC DESIGNATION SUR DEUX LIGNES", 50),
        '    .refMag = "2002-9999",
        '    .colisage = 1,
        '    .emplacement = "B10 E10 R10 N10",
        '    .gencodeFourn = "370009223482",
        '    .gencodeMag = "370009223482",
        '    .refFourn = "0-33-2389999999F",
        '    .prix = "1300,10",
        '    .count = 1})

        PrintNormalLabels(pLabels)
    End Sub

    Private WithEvents LabelsDocToPrint As New Printing.PrintDocument
    
    Private Sub MakeLabelsToPrint()
        LabelsToPrint = New ArrayList
        For Each label As PrintLabel In pLabels
            For i As Integer = 1 To label.count
                LabelsToPrint.Add(label)
            Next
        Next
    End Sub

    Private Sub PrintNormalLabels(pLabels As List(Of PrintLabel))
        MakeLabelsToPrint
        Dim dialogPrint As New PrintPreviewDialogSelectPrinter
        AddHandler dialogPrint.MyPrintItemClick, AddressOf MyLabelsPrintItemClicked
        dialogPrint.Document = LabelsDocToPrint
        dialogPrint.ShowDialog()
    End Sub

    Private Sub MyLabelsPrintItemClicked()
        MakeLabelsToPrint
    End Sub

    Private Sub LabelsDocToPrint_PrintPage(sender As Object, e As PrintPageEventArgs) Handles LabelsDocToPrint.PrintPage
        Dim x As Integer = 0
        Dim y As Integer = 0
        Dim breakPage As Boolean
        Dim pLabel As PrintLabel
        Dim pFormat As FormatNormal
        Dim maxWidth As Integer = LabelsDocToPrint.DefaultPageSettings.PaperSize.Width
        If (maxWidth Mod 2) <> 0
            maxWidth -= 1
        End If
        Dim maxHeight As Integer = LabelsDocToPrint.DefaultPageSettings.PaperSize.Height

        LabelsToPrint.Reverse
        For i As Integer = LabelsToPrint.Count - 1 To 0 Step -1
            pLabel = LabelsToPrint(i)
            pFormat = pLabel.format

            breakPage = pFormat.DrawLabel(pLabel, e.Graphics, maxWidth, maxHeight, x, y)
            LabelsToPrint.RemoveAt(i)

            If breakPage
                Exit For
            End If
        Next
        e.HasMorePages = LabelsToPrint.Count > 0
    End Sub
End Class
