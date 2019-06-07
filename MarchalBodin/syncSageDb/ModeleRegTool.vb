Imports System.Data.SqlClient
Imports System

Public Class ModeleRegTool
    Inherits Tool

    ' List of MODREGCHKSYNCVIEW.COLUMNS
    Private vCols As New List(Of String) From {"MR_No", "CS", "csModeleR", "csEModeleRAgg"}

    Public Sub New(ByRef dbTarget As Database, ByRef dbSource As Database)
        MyBase.New(dbTarget, dbSource)
    End Sub

    Public Sub MajModReg()
        Try
            Log("[MAJ MOD REG]")

            Using cnxSource As New SqlConnection(dbTarget.ToString)
                Using cmd As SqlCommand = cnxSource.CreateCommand
                    cnxSource.Open()
                    ' Requete ~= 12 secondes Récup les différences entre les bases

                    cmd.CommandText = String.Format(
                        "SELECT {0} 
                        FROM {1}.[MODREGCHKSYNCVIEW] L
                        LEFT JOIN {2}.[MODREGCHKSYNCVIEW] R On R.MR_No = L.MR_No 
                        WHERE L.CS <> R.CS Or R.MR_No Is NULL
                        ORDER BY L.MR_No",
                        GetViewSelectAliases(vCols),
                        dbSource.sqlDbName,
                        dbTarget.sqlDbName
                    )
                    cmd.CommandTimeout = cmdTimeOut

                    Using reader As SqlDataReader = cmd.ExecuteReader
                        If reader.HasRows Then
                            While reader.Read
                                checkModReg(reader)
                            End While
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Log(ex.ToString)
        End Try
    End Sub

    Private Function checkModReg(ByRef viewModReg As SqlDataReader) As Boolean
        Dim mrNo As String = viewModReg.Item("L_MR_No")
        Dim insert As Boolean = viewModReg.Item("R_CS") = 0
        Log(String.Format("[{0}]", mrNo))

        Using cnxTarget As New SqlConnection(dbTarget.ToString)
            cnxTarget.Open()
            Dim transaction As SqlTransaction = cnxTarget.BeginTransaction
            Try

                Using cnxSource As New SqlConnection(dbSource.ToString)
                    cnxSource.Open()

                    '
                    ' F_MODELER
                    '
                    ' Champs à renseigner obligatoirement lors de l’ajout: MR_No, MR_Intitule
                    ' Champs non modifiables: MR_No
                    If viewModReg.Item("L_csModeleR").ToString <> viewModReg.Item("R_csModeleR").ToString Then
                        Using cmdSource As SqlCommand = cnxSource.CreateCommand
                            cnxSource.Open()
                            cmdSource.CommandText = String.Format("SELECT [MR_Intitule] FROM {0}.[F_MODELER] WHERE MR_No = @mrNo", dbSource.sqlDbName)
                            cmdSource.Parameters.AddWithValue("@mrNo", mrNo)
                            cmdSource.CommandTimeout = cmdTimeOut

                            Using reader As SqlDataReader = cmdSource.ExecuteReader(CommandBehavior.SingleResult)
                                While reader.Read
                                    Using cmdTarget As SqlCommand = cnxTarget.CreateCommand
                                        With cmdTarget
                                            .Transaction = transaction
                                            If insert Then
                                                .CommandText = String.Format("INSERT INTO {0}.[dbo].[F_MODELER] (MR_No, MR_Intitule) VALUES (@mrNo, @mrIntitule)", cnxTarget.Database)
                                            Else
                                                .CommandText = String.Format("UPDATE {0}.[dbo].[F_MODELER] Set MR_Intitule = @mrIntitule WHERE MR_No = @mrNo", cnxTarget.Database)
                                            End If
                                            .Parameters.AddWithValue("@mrNo", mrNo)
                                            .Parameters.AddWithValue("@mrIntitule", reader.Item("MR_Intitule"))
                                            .CommandTimeout = cmdTimeOut
                                            .ExecuteNonQuery()
                                        End With
                                    End Using
                                End While
                            End Using
                        End Using
                    End If

                    '
                    ' F_EMODELER
                    '
                    If viewModReg.Item("L_csEModeleRAgg").ToString <> viewModReg("R_csEModeleRAgg").ToString Then
                        Delete("F_EMODELER", transaction, "MR_No", mrNo)
                        CopySourceToTarget(
                            cnxSource,
                            "F_EMODELER",
                            New List(Of String) From {"MR_No", "N_Reglement", "ER_Condition", "ER_NbJour", "ER_JourTb01", "ER_JourTb02",
                                "ER_JourTb03", "ER_JourTb04", "ER_JourTb05", "ER_JourTb06", "ER_TRepart", "ER_VRepart"},
                            mrNo,
                            transaction
                        )
                    End If
                End Using ' End cnxSource

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
End Class
