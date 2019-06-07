Public Class PrintLabel
    Public format As IFormat
    Public refMag As String
    Public designation As String
    Public prix As String
    Public colisage As Integer
    Public refFourn As String
    Public emplacement As String
    Public emplSec As String
    Public dateImpr As String = Date.Now.ToShortDateString
    Public gencodeFourn As String
    Public gencodeMag As String
    Public count As Integer
    Public conditionnement As String
End Class
