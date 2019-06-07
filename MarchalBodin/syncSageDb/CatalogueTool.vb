Imports System.Data.SqlClient
Public Class CatalogueTool
    Inherits Tool

    ' List of CATCHKSYNCVIEW.COLUMNS
    Private vCols As New List(Of String) From {"CL_No", "csCatalogue", "cbModification", "CL_Niveau"}
    Public Sub New(ByRef dbTarget As Database, ByRef dbSource As Database)
        MyBase.New(dbTarget, dbSource)
    End Sub

    ' Crée ou met à jour les catégories
    Public Sub CreateOrUpdateCatalogue()
        Try
            Log("[MAJ CATALOGUE]")

            Using cnxSource As New SqlConnection(dbSource.ToString)
                Using cmd As SqlCommand = cnxSource.CreateCommand
                    cnxSource.Open()
                    cmd.CommandText = String.Format(
                        "SELECT {0} 
                        FROM {1}.[CATCHKSYNCVIEW] L 
                        LEFT JOIN {2}.[CATCHKSYNCVIEW] R ON L.CL_No = R.CL_No 
                        WHERE L.csCatalogue <> R.csCatalogue OR R.CL_No IS NULL 
                        ORDER BY L.CL_Niveau",
                        GetViewSelectAliases(vCols),
                        dbSource.sqlDbName,
                        dbTarget.sqlDbName
                    )
                    cmd.CommandTimeout = cmdTimeOut

                    Using reader As SqlDataReader = cmd.ExecuteReader
                        If reader.HasRows Then
                            While reader.Read
                                checkCategorie(reader)
                            End While
                        End If
                    End Using

                End Using
            End Using
        Catch ex As Exception
            Log(ex.ToString)
        End Try
    End Sub

    Private Function checkCategorie(ByRef viewCat As SqlDataReader) As Boolean
        Dim clNo As String = viewCat.Item("L_CL_No")
        Dim insert As Boolean = viewCat.Item("R_csCatalogue") = 0
        Log(String.Format("[{0}]", clNo))

        Using cnxTarget As New SqlConnection(dbTarget.ToString)
            cnxTarget.Open()
            Dim transaction As SqlTransaction = cnxTarget.BeginTransaction
            Try
                Using cnxSource As New SqlConnection(dbSource.ToString)
                    Using cmdSource As SqlCommand = cnxSource.CreateCommand
                        cnxSource.Open()
                        cmdSource.CommandText = String.Format(
                            "SELECT [CL_Intitule], [CL_Code], [CL_Stock], [CL_NoParent], [CL_Niveau]
                            FROM {0}.[F_CATALOGUE] WHERE CL_No = @clNo",
                            dbSource.sqlDbName
                        )
                        cmdSource.Parameters.AddWithValue("@clNo", clNo)
                        cmdSource.CommandTimeout = cmdTimeOut

                        Using reader As SqlDataReader = cmdSource.ExecuteReader(CommandBehavior.SingleRow)
                            While reader.Read
                                ' S'assure qu'il n'y aura pas de doublons de parent CL_No+CL_Intitule
                                Using cmdTarget As SqlCommand = cnxTarget.CreateCommand
                                    With cmdTarget
                                        .Transaction = transaction
                                        .CommandText = String.Format("UPDATE {0}.[dbo].[F_CATALOGUE] SET CL_Intitule = 'TEMPSYNC' + @clNo WHERE CL_NoParent = @clNoParent AND CL_Intitule = @clIntitule", cnxTarget.Database)
                                        .Parameters.AddWithValue("@clNo", clNo)
                                        .Parameters.AddWithValue("@clNoParent", reader.Item("CL_NoParent"))
                                        .Parameters.AddWithValue("@clIntitule", reader.Item("CL_Intitule"))
                                        .CommandTimeout = cmdTimeOut
                                        .ExecuteNonQuery()
                                    End With
                                End Using

                                Using cmdTarget As SqlCommand = cnxTarget.CreateCommand
                                    With cmdTarget
                                        .Transaction = transaction
                                        If insert Then
                                            Dim fields As String = "CL_No"
                                            Dim values As String = "@clNo"

                                            For i As Integer = 0 To reader.FieldCount - 1
                                                fields &= String.Format(", {0}", reader.GetName(i))
                                                values &= String.Format(", @{0}", reader.GetName(i))
                                                .Parameters.AddWithValue(reader.GetName(i), reader.Item(i))
                                            Next
                                            .CommandText = String.Format("INSERT INTO {0}.[dbo].[F_CATALOGUE] ({1}) VALUES ({2})", cnxTarget.Database, fields, values)
                                        Else
                                            ' Update
                                            .CommandText = String.Format("UPDATE {0}.[dbo].[F_CATALOGUE] SET ", cnxTarget.Database)
                                            For i As Integer = 0 To reader.FieldCount - 1
                                                .CommandText &= String.Format("[{0}] = @{0}", reader.GetName(i))
                                                .Parameters.AddWithValue(reader.GetName(i), reader.Item(i))

                                                If i <> reader.FieldCount - 1 Then
                                                    .CommandText &= ", "
                                                End If
                                            Next
                                            .CommandText &= " WHERE CL_No = @clNo"
                                        End If
                                        .Parameters.AddWithValue("@clNo", clNo)
                                        .CommandTimeout = cmdTimeOut
                                        .ExecuteNonQuery()
                                    End With
                                End Using
                            End While
                        End Using
                    End Using
                End Using

                transaction.Commit()
                Return True
            Catch ex As Exception
                Log(String.Format("Commit Exception: {0}", ex.ToString))

                ' Attempt to roll back the transaction. 
                Try
                    transaction.Rollback()
                    Return False
                Catch ex2 As Exception
                    Log(String.Format("Rollback Exception Type: {0}", ex2.ToString))
                    Return False
                End Try
            End Try
        End Using
    End Function

    ' Supprime les catégories qui ne sont plus présentes dans TARIF
    Public Function DeleteCatalogue() As Boolean
        ' TODO Ne doit fonctionner que dans un sens pour le moment
        If dbTarget.dbName Like "TARIF*" Then
            Log("Pas de delete catalogue sur une base tarif")
            Return True
        End If
        Log("[DEL CATALOGUE]")
        Using cnxSource As New SqlConnection(dbTarget.ToString)
            cnxSource.Open()
            'Dim transaction As SqlTransaction = cnxSource.BeginTransaction
            Try
                For i As Integer = 4 To 1 Step -1
                    With cnxSource.CreateCommand
                        '.Transaction = transaction
                        .CommandText = String.Format(
                            "DELETE L FROM {0}.[F_CATALOGUE] L 
                            WHERE L.CL_No NOT IN(SELECT R.CL_No FROM {1}.[F_CATALOGUE] R) AND CL_Niveau = @clNiv",
                            dbTarget.sqlDbName,
                            dbSource.sqlDbName
                        )
                        .Parameters.AddWithValue("@clNiv", i)
                        .ExecuteNonQuery()
                    End With
                Next
                Return True
            Catch ex As Exception
                Log(String.Format("Commit Exception: {0}", ex.ToString))
                Return False
            End Try
        End Using
    End Function
End Class
