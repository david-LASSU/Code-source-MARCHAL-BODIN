Public Class Database
    Private _server As String
    Public Property server As String
        Get
            Return _server
        End Get
        Set(value As String)
            _server = value
        End Set
    End Property

    Private _dbName As String
    Public Property dbName As String
        Get
            Return _dbName
        End Get
        Set(value As String)
            _dbName = value
        End Set
    End Property

    Private _isError As Boolean
    Public Property isError As Boolean
        Get
            Return _isError
        End Get
        Set(value As Boolean)
            _isError = value
        End Set
    End Property

    Public ReadOnly Property hostname As String
    Get
            Return server.Split(".").First
    End Get
    End Property

    ' Raccourcis vers [hostname].[database].[dbo]
    Public ReadOnly Property sqlDbName As String
        Get
            Return String.Format("[{0}].[{1}].[dbo]", server, dbName)
        End Get
    End Property
    Public Overrides Function ToString() As String
        Return String.Format("server={0};Trusted_Connection=True;Database={1};MultipleActiveResultSets=True", server, dbName)
        'Return String.Format("Data Source={0};Network Library=DBMSSOCN;Initial Catalog={1};User ID=sa;Password=58xxnuk!;MultipleActiveResultSets=True", server, dbName)
    End Function

    Public Sub New(Byval server As String, ByVal dbName As String)
        Me.server = server
        Me.dbName = dbName
    End Sub
End Class
