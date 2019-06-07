Imports System.Data.SqlClient

Public Class DatabaseCollection
    Inherits System.Collections.CollectionBase

    '    Public Sub New()
    '#If DEBUG Then
    '        Me.List.Add(New Database("SRVSQL01", "SMODEV", "58xxnuk!"))
    '        'Me.List.Add(New Database("SRVSQL01", "TARIFDEV", "58xxnuk!"))
    '        'Me.List.Add(New Database("SRVSQL02.MARCHAL-BODIN.LOCAL", "BODINDEV", "58xxnuk!"))
    '#Else
    '        ' Add production databases here
    '        Me.List.Add(New Database("SRVSQL01", "SMO", "58xxnuk!"))
    '        Me.List.Add(New Database("SRVSQL02.MARCHAL-BODIN.LOCAL", "BODIN", "58xxnuk!"))
    '        Me.List.Add(New Database("SRVSQL03.MARCHAL-BODIN.LOCAL", "SOBRIGIR", "58xxnuk!"))

    '        Me.List.Add(New Database("SRVSQL02.MARCHAL-BODIN.LOCAL", "SOGEDIS", "58xxnuk!"))
    '        Me.List.Add(New Database("SRVSQL02.MARCHAL-BODIN.LOCAL", "PPJ", "58xxnuk!"))
    '        Me.List.Add(New Database("SRVSQL01", "CDA", "58xxnuk!"))
    '#End If
    '    End Sub
    'Public Sub New(ByRef reader As SqlDataReader)
    '    While reader.Read
    '        Me.List.Add
    '    End While
    'End Sub
End Class
