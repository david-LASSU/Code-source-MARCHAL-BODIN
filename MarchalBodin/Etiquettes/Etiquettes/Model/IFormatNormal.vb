Public Interface IFormatNormal
    ''' <summary>
    ''' Return True if break page
    ''' </summary>
    ''' <param name="pLabel"></param>
    ''' <param name="g"></param>
    ''' <param name="maxWidth"></param>
    ''' <param name="maxHeight"></param>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <returns></returns>
    Function DrawLabel(ByRef pLabel As PrintLabel, ByRef g As Graphics, ByRef maxWidth As Integer, ByRef maxHeight As Integer, ByRef x As Integer, ByRef y As Integer) As Boolean
End Interface
