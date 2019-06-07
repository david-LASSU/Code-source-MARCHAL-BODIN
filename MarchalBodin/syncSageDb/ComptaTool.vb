Imports System.Data.SqlClient

Public Class ComptaTool
    Inherits Tool

    ' List of CATCHKSYNCVIEW.COLUMNS
    Private vCols As New List(Of String) From {"CG_Num", "csCompteG", "cbModification"}

    Public Sub New(ByRef dbTarget As Database, ByRef dbSource As Database)
        MyBase.New(dbTarget, dbSource)
    End Sub

    Public Sub MajCompta()
        Try
            Log("[MAJ COMPTA]")
            Using cnxSource As New SqlConnection(dbTarget.ToString)
                Using cmd As SqlCommand = cnxSource.CreateCommand
                    cnxSource.Open()
                    cmd.CommandText = String.Format(
                        "SELECT {0} 
                        FROM {1}.[CPTGCHKSYNCVIEW] L
                        LEFT JOIN {2}.[CPTGCHKSYNCVIEW] R On L.CG_Num = R.CG_Num
                        WHERE R.CG_Num Is NULL ",
                        GetViewSelectAliases(vCols),
                        dbSource.sqlDbName,
                        dbTarget.sqlDbName)

                    cmd.CommandTimeout = cmdTimeOut

                    Using reader As SqlDataReader = cmd.ExecuteReader
                        If reader.HasRows Then
                            While reader.Read
                                checkCompta(reader)
                            End While
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Log(ex.ToString)
        End Try
    End Sub

    Private Function checkCompta(ByRef viewCompta As SqlDataReader) As Boolean
        Dim cgNum As String = viewCompta.Item("L_CG_Num")
        Dim insert As Boolean = viewCompta.Item("R_csCompteG") = 0

        ' Ne fonctionne que pour l'insert
        If insert = False Then
            Return True
        End If
        Log(String.Format("[{0}]", cgNum))

        Using cnxTarget As New SqlConnection(dbTarget.ToString)
            cnxTarget.Open()
            Dim transaction As SqlTransaction = cnxTarget.BeginTransaction

            Try
                Using cnxSource As New SqlConnection(dbSource.ToString)
                    cnxSource.Open()
                    Using cmdSource As SqlCommand = GetSelectCmdFromCnx(
                        cnxSource,
                        "F_COMPTEG",
                        New List(Of String) From {"CG_Num", "CG_Type", "CG_Intitule", "CG_Classement", "N_Nature", "CG_Report", "CR_Num", "CG_Raccourci", "CG_Saut", "CG_Regroup", "CG_Analytique",
                            "CG_Echeance", "CG_Quantite", "CG_Lettrage", "CG_Tiers", "CG_DateCreate", "CG_Devise", "N_Devise", "TA_Code", "CG_Sommeil", "CG_ReportAnal"},
                        cgNum)

                        Using reader As SqlDataReader = cmdSource.ExecuteReader(CommandBehavior.SingleRow)
                            While reader.Read
                                InsertRowFromReader("F_COMPTEG", reader, transaction)
                            End While
                        End Using
                    End Using
                End Using

                transaction.Commit()
                Return True
            Catch ex As Exception
                Log(String.Format("Commit Exception: {0}", ex.ToString))
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
End Class
