Public Class FormatCollection
    Inherits System.Collections.CollectionBase

    ''' <summary>
    ''' Crée une collection de formats
    ''' 
    ''' Définition des arguments dans les chaines:
    ''' 0 = Designation
    ''' 1 = Ref mag
    ''' 2 = Colisage
    ''' 3 = Emplacement
    ''' 4 = Ref Fourn
    ''' 5 = Prix TTC
    ''' </summary>
    Public Sub New()
        ' Format 1
        List.Add(New FormatZebra() With {
            .key = "FORMAT_1",
            .description = "57x35 cartonnée",
            .zplString = "^XA
                        ^FX Print speed
                        ^PRA
                        ^FX Font Encoding
                        ^CI28
                        ^PW 456
                        ^FX Désignation
                        ^FO10,15^ARN^FB400,4,,^FD{0}^FS
                        ^FX REF
                        ^FO10,107^FR^A0N,50,30^FD{1}^FS
                        ^FX Colisage
                        ^FO10,175^ADN,26,14^FD{2}^FS
                        ^FX Emplacement
                        ^FO230,100^A0N,30,30^FD{3}^FS
                        ^FX Ref fourn
                        ^FO230,155^ADN,26,14^FD{4}^FS
                        ^FX Code barre Fournisseur
                        ^FO10,155^ADN,26,14^FD{5}^FS
                        ^FX EAN
                        ^FO70,200^BY2{7}
                        ^FX Emplacement 2ndaire
                        ^FO230,128^A0N,27,27^FD{9}^FS
                        ^FX Date
                        ^FO230,175^ADN,26,14^FD{10}^FS
                        ^XZ",
             .B3String = "^B3N,N,30,N,N^FD{0}^FS",
             .BEString = "^BEN,30,N,N^FD{0}^FS",
             .url = "http://api.labelary.com/v1/printers/8dpmm/labels/2.2441x1.378/0/",
             .width = 456,
             .height = 280})

        ' Format 2
        List.Add(New FormatZebra() With {
            .key = "FORMAT_2",
            .description = "57x35 cartonnée avec prix",
            .zplString = "^XA
                ^FX Print speed
                ^PRA
                ^FX Font Encoding
                ^CI28
                ^PW 456
                ^FX Désignation
                ^FO10,15^ARN^FB400,4,,^FD{0}^FS
                ^FX REF
                ^FO10,80^FR^A0N,50,30^FD{1}^FS
                ^FX Colisage
                ^FO10,153^ADN,26,14^FD{2}^FS
                ^FX Emplacement principal
                ^FO230,80^A0N,30,30^FD{3}^FS
                ^FX Ref fourn
                ^FO230,135^ADN,26,14^FD{4}^FS
                ^FX Code barre fournisseur
                ^FO10,130^ADN,26,14^FD{5}^FS
                ^FX EAN
                ^FO80,155^BY2
                {7}
                ^FX Prix noir sur blanc
                ^FO75,195^GB360,62,36^FS
                ^CI0,21,36
                ^FO85,205^FR^A0N,55,45^FD{6} $^FS
                ^FO275,205^FR^ADN,26,14^FD Prix TTC^FS
                ^FX Conditionnement
                ^FO275,230^FR^ADN,26,14^FD{8}^FS
                ^FX Emplacement 2ndaire
                ^FO230,105^A0N,27,27^FD{9}^FS
                ^FX Date
                ^FO10,175^ADN,26,14^FD{10}^FS
                ^XZ",
            .B3String = "^B3N,N,30,N,N^FD{0}^FS",
            .BEString = "^BEN,30,N,N^FD{0}^FS",
            .url = "http://api.labelary.com/v1/printers/8dpmm/labels/2.2441x1.378/0/",
            .width = 456,
            .height = 280})

        ' Format Papier A4 39x105
        List.Add(New FormatNormal() With {
            .key = FormatNormal.FORMAT_3,
            .description = "Sur papier A4 39x105",
            .width = 393,
            .height = 151})

        ' Format A5 
        List.Add(New FormatNormal() With {
            .key = FormatNormal.FORMAT_4,
            .description = "A5 sur papier A4",
            .width = 826,
            .height = 584})

        ' Format A5 avec prix
        List.Add(New FormatNormal() With {
            .key = FormatNormal.FORMAT_5,
            .description = "A5 sur papier A4 avec prix",
            .width = 826,
            .height = 584})

        ' Format Special double foyer
        List.Add(New FormatZebra() With {
            .key = "FORMAT_GC",
            .description = "57x35 cartonnée gros caractères",
            .zplString = "^XA
                    ^FX Print speed
                    ^PRA
                    ^FX Font Encoding
                    ^CI28
                    ^PW 456
                    ^FX REF
                    ^FO30,15^FR^A0N,200,80^FD{1}^FS

                    ^FX EAN
                    ^FO70,190^BY2
                    {7}
                    ^XZ",
            .B3String = "^B3N,N,50,N,N^FD{0}^FS",
            .BEString = "^BEN,50,N,N^FD{0}^FS",
            .url = "http://api.labelary.com/v1/printers/8dpmm/labels/2.2441x1.378/0/",
            .width = 456,
            .height = 280})

        ' Format CB
        List.Add(New FormatZebra() With {
            .key = "FORMAT_CB",
            .description = "Autocollantes 32x25",
            .zplString = "^XA
                    ^FX Print speed
                    ^PRA
                    ^FX Font Encoding
                    ^CI28
                    ^FX Print Width default 254
                    ^PW 254
                    ^FX REF
                    ^FO0,20^A0N,35,25^FB400,4,,^FD{1}^FS
                    ^FX EAN
                    ^FO0,55^BY1
                    {7}
                    ^XZ",
            .B3String = "^B3N,N,50,Y,N^FD{0}^FS",
            .BEString = "^BEN,50,N,N^FD{0}^FS",
            .url = "http://api.labelary.com/v1/printers/8dpmm/labels/1.2204x0.9842/0/",
            .width = 325,
            .height = 200})

        ' Format CB PX
        List.Add(New FormatZebra() With {
            .key = "FORMAT_CBPX",
            .description = "Autocollantes 32x25 avec prix",
            .zplString = "^XA
                    ^FX Print speed
                    ^PRA
                    ^FX Font Encoding
                    ^CI28
                    ^FX Print Width default 254
                    ^PW 254
                    ^FX REF
                    ^FO0,20^A0N,35,25^FB400,4,,^FD{1}^FS
                    ^FX EAN
                    ^FO0,55^BY1
                    {7}
                    ^FX Prix noir sur blanc
                    ^FO0,155^GB300,72,36^FS
                    ^FO10,165^FR^A0N,40,30^FD{6}^FS
                    ^CI0,21,36
                    ^FO100,165^FR^ADN,36,20^FD $TTC^FS
                    ^XZ",
            .B3String = "^B3N,N,50,Y,N^FD{0}^FS",
            .BEString = "^BEN,50,N,N^FD{0}^FS",
            .url = "http://api.labelary.com/v1/printers/8dpmm/labels/1.2204x0.9842/0/",
            .width = 325,
            .height = 200})
    End Sub

    Public Function getFormat(ByVal key As String) As Format
        For Each frm As Format In Me
            If frm.key = key Then
                Return frm
            End If
        Next

        Throw New Exception(String.Format("Format '{0}' not found", key))
    End Function

    Public Function hasFormat(ByVal key As String) As Boolean
        For Each frm As Format In Me
            If frm.key = key Then
                Return True
            End If
        Next

        Return False
    End Function
End Class
