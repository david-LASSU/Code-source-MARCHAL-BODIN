Imports System.Data.SqlClient
Imports System.IO

Public Class GlossaireTool
    Inherits Tool

    Private vCols As New List(Of String) From {"GL_No", "CS", "csGlossaire", "csArtGlossAgg"}

    Public Sub New(ByRef dbTarget As Database, ByRef dbSource As Database)
        MyBase.New(dbTarget, dbSource)
    End Sub

    Public Sub MajGlossaire()
        Try
            Log("[MAJ GLOSSAIRE]")

            Using cnxSource As New SqlConnection(dbTarget.ToString)
                cnxSource.Open()
                Using cmd As SqlCommand = cnxSource.CreateCommand()
                    cmd.CommandText = String.Format(
                        "SELECT {0}
                        FROM {1}.[GLOSSCHKSYNCVIEW] L 
                        LEFT JOIN {2}.[GLOSSCHKSYNCVIEW] R ON L.GL_No = R.GL_No 
                        WHERE L.CS <> R.CS OR R.GL_No IS NULL",
                        GetViewSelectAliases(vCols),
                        dbSource.sqlDbName,
                        dbTarget.sqlDbName
                    )
                    cmd.CommandTimeout = cmdTimeOut

                    Using reader As SqlDataReader = cmd.ExecuteReader
                        If reader.HasRows Then
                            While reader.Read
                                CheckGlossaire(reader)
                            End While
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Log(ex.ToString())
        End Try
    End Sub

    Private Function CheckGlossaire(ByRef viewGloss As SqlDataReader) As Boolean
        Dim GLNo As String = viewGloss.Item("L_GL_No")
        Dim insert As Boolean = viewGloss.Item("R_CS") = 0

        Log(String.Format("[{0}] {1}", GLNo, If(insert, "INSERT", "UPDATE")))

        Using cnxTarget As New SqlConnection(dbTarget.ToString)
            cnxTarget.Open()

            Dim transaction As SqlTransaction = cnxTarget.BeginTransaction()

            Try
                Using cnxSource As New SqlConnection(dbSource.ToString)
                    cnxSource.Open()

                    '
                    ' F_GLOSSAIRE
                    '
                    If viewGloss.Item("L_csGlossaire").ToString() <> viewGloss.Item("R_csGlossaire").ToString() Then
                        Using cmdSource As SqlCommand = GetSelectCmdFromCnx(
                            cnxSource,
                            "F_GLOSSAIRE",
                            New List(Of String)() From {"GL_No", "GL_Domaine","GL_Intitule","GL_Raccourci","GL_PeriodeDeb","GL_PeriodeFin","GL_Text"}, 
                            GLNo
                        )
                            Using reader As SqlDataReader = cmdSource.ExecuteReader(CommandBehavior.SingleRow)
                                reader.Read()

                                If insert Then
                                    InsertRowFromReader("F_GLOSSAIRE",reader, transaction)
                                Else 
                                    Using cmdTarget As SqlCommand = cnxTarget.CreateCommand()
                                        With cmdTarget
                                            .Transaction = transaction

                                            .CommandText = String.Format("UPDATE {0}.[dbo].[F_GLOSSAIRE] SET ", cnxTarget.Database)
                                            For i As Integer = 0 To reader.FieldCount - 1
                                                If reader.GetName(i) = "GL_No" Then 
                                                    Continue For
                                                End If

                                                .CommandText &= String.Format("[{0}] = @{0}", reader.GetName(i))
                                                .Parameters.AddWithValue(reader.GetName(i), reader.Item(i))

                                                If i <> reader.FieldCount - 1 Then
                                                    .CommandText &= ", "
                                                End If
                                            Next
                                            .CommandText &= " WHERE GL_No = @GL_No"
                                            .Parameters.AddWithValue("@GL_No", GLNo)
                                            .CommandTimeout = cmdTimeOut
                                            .ExecuteNonQuery()
                                        End With
                                    End Using
                                End If
                            End Using
                        End Using
                    End If

                    '
                    ' F_ARTGLOSS
                    '
                    If viewGloss.Item("L_csArtGlossAgg").ToString() <> viewGloss.Item("R_csArtGlossAgg").ToString() Then
                        ' DELETE glossaires articles
                        Delete("F_ARTGLOSS", transaction, "GL_No", GLNo)
                        ' Recréation des glossaires articles
                        CopySourceToTarget(
                            cnxSource,
                            "F_ARTGLOSS",
                            New List(Of String)() From {"GL_No", "AR_Ref", "AGL_Num"},
                            GLNo,
                            transaction
                        )
                    End If

                End Using

                transaction.Commit()
                Return True
            Catch ex As Exception
                Log(String.Format("Commit Exception : {0}", ex.ToString))

                ' Attempt to roll back the transaction. 
                Try
                    transaction.Rollback()
                    Return False
                Catch ex2 As Exception
                    ' This catch block will handle any errors that may have occurred 
                    ' on the server that would cause the rollback to fail, such as 
                    ' a closed connection.
                    Log(String.Format("Rollback Exception Type: {0}", ex2.ToString))
                    Return False
                End Try
            End Try
        End Using
    End Function

    Public Function DeleteGlossaire() As Boolean
        If dbTarget.dbName Like "TARIF*" Then
            Log("Pas de delete famille sur une base tarif")
            Return True
        End If

        Log("[DEL GLOSSAIRE]")
        Using cnxSource As New SqlConnection(dbTarget.ToString)
            Try
                cnxSource.Open()
                Dim tables As New List(Of String) From {"F_ARTGLOSS", "F_GLOSSAIRE"}

                For Each table As String In tables
                    With cnxSource.CreateCommand
                        .CommandText = String.Format(
                            "DELETE L FROM {0}.[{1}] L WHERE L.GL_No NOT IN(SELECT R.GL_No FROM {2}.[F_GLOSSAIRE] R)",
                            dbTarget.sqlDbName,
                            table,
                            dbSource.sqlDbName
                        )
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
