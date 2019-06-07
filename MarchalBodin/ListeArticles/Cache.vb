Public Class Cache
    Private Shared RowsPerPage As Integer

    ' Represents one page of data.  
    Public Structure DataPage

        Public table As DataTable
        Private lowestIndexValue As Integer
        Private highestIndexValue As Integer

        Public Sub New(ByVal table As DataTable, ByVal rowIndex As Integer)
            Me.table = table
            lowestIndexValue = MapToLowerBoundary(rowIndex)
            highestIndexValue = MapToUpperBoundary(rowIndex)
            System.Diagnostics.Debug.Assert(lowestIndexValue >= 0)
            System.Diagnostics.Debug.Assert(highestIndexValue >= 0)
        End Sub

        Public ReadOnly Property LowestIndex() As Integer
            Get
                Return lowestIndexValue
            End Get
        End Property

        Public ReadOnly Property HighestIndex() As Integer
            Get
                Return highestIndexValue
            End Get
        End Property

        Public Shared Function MapToLowerBoundary(
            ByVal rowIndex As Integer) As Integer

            ' Return the lowest index of a page containing the given index.
            Return (rowIndex \ RowsPerPage) * RowsPerPage

        End Function

        Private Shared Function MapToUpperBoundary(
            ByVal rowIndex As Integer) As Integer

            ' Return the highest index of a page containing the given index.
            Return MapToLowerBoundary(rowIndex) + RowsPerPage - 1

        End Function

    End Structure

    Private cachePages As DataPage()
    Private dataSupply As IDataPageRetriever

    Public Sub New(ByVal dataSupplier As IDataPageRetriever,
        ByVal rowsPerPage As Integer)

        dataSupply = dataSupplier
        Cache.RowsPerPage = rowsPerPage
        LoadFirstTwoPages()

    End Sub

    ' Sets the value of the element parameter if the value is in the cache.
    Private Function IfPageCached_ThenSetElement(ByVal rowIndex As Integer, ByVal columnIndex As Integer, ByRef element As String) As Boolean
        If (cachePages(0).table.Rows.Count = 0) Then
            Return False
        End If
        Dim rowIndexModRowsPerPage As Integer = rowIndex Mod RowsPerPage

        If IsRowCachedInPage(0, rowIndex) Then
            element = cachePages(0).table.Rows(rowIndexModRowsPerPage).Item(columnIndex).ToString()
            Return True
        ElseIf IsRowCachedInPage(1, rowIndex) Then
            element = cachePages(1).table.Rows(rowIndexModRowsPerPage).Item(columnIndex).ToString()
            Return True
        End If

        Return False

    End Function

    Public Function RetrieveElement(ByVal rowIndex As Integer, ByVal columnIndex As Integer) As String

        Dim element As String = Nothing
        If IfPageCached_ThenSetElement(rowIndex, columnIndex, element) Then
            Return element
        Else
            Return RetrieveData_CacheIt_ThenReturnElement(rowIndex, columnIndex)
        End If

    End Function

    Private Sub LoadFirstTwoPages()

        cachePages = New DataPage() {
            New DataPage(dataSupply.SupplyPageOfData(
                DataPage.MapToLowerBoundary(0), RowsPerPage), 0),
            New DataPage(dataSupply.SupplyPageOfData(
                DataPage.MapToLowerBoundary(RowsPerPage),
                RowsPerPage), RowsPerPage)
        }

    End Sub

    Private Function RetrieveData_CacheIt_ThenReturnElement(ByVal rowIndex As Integer, ByVal columnIndex As Integer) As String

        ' Retrieve a page worth of data containing the requested value.
        Dim table As DataTable = dataSupply.SupplyPageOfData(DataPage.MapToLowerBoundary(rowIndex), RowsPerPage)
        If table.Rows.Count = 0 Then
            Return String.Empty
        End If
        ' Replace the cached page furthest from the requested cell
        ' with a new page containing the newly retrieved data.
        cachePages(GetIndexToUnusedPage(rowIndex)) = New DataPage(table, rowIndex)

        Return RetrieveElement(rowIndex, columnIndex)

    End Function

    ' Returns the index of the cached page most distant from the given index
    ' and therefore least likely to be reused.
    Private Function GetIndexToUnusedPage(ByVal rowIndex As Integer) As Integer

        If rowIndex > cachePages(0).HighestIndex AndAlso
            rowIndex > cachePages(1).HighestIndex Then

            Dim offsetFromPage0 As Integer =
                rowIndex - cachePages(0).HighestIndex
            Dim offsetFromPage1 As Integer =
                rowIndex - cachePages(1).HighestIndex
            If offsetFromPage0 < offsetFromPage1 Then
                Return 1
            End If
            Return 0
        Else
            Dim offsetFromPage0 As Integer =
                cachePages(0).LowestIndex - rowIndex
            Dim offsetFromPage1 As Integer =
                cachePages(1).LowestIndex - rowIndex
            If offsetFromPage0 < offsetFromPage1 Then
                Return 1
            End If
            Return 0
        End If

    End Function

    ' Returns a value indicating whether the given row index is contained
    ' in the given DataPage. 
    Private Function IsRowCachedInPage(
        ByVal pageNumber As Integer, ByVal rowIndex As Integer) As Boolean

        Return rowIndex <= cachePages(pageNumber).HighestIndex AndAlso
            rowIndex >= cachePages(pageNumber).LowestIndex

    End Function
End Class
