Imports System.Data.SqlClient

Public Class FamilleTool
    Inherits Tool

    Public Sub New(ByRef dbTarget As Database, ByRef dbSource As Database)
        MyBase.New(dbTarget, dbSource)
    End Sub

    Private vCols As New List(Of String) From {"FA_CodeFamille", "csFamille", "csFamComptaAgg", "csFamFournissAgg", "csFamTarifAgg", "csFamTarifQteAgg", "CS"}

    Public Sub MajFamilles()
        Log("[MAJ FAMILLES]")
        Try
            Using cnxSource As New SqlConnection(dbTarget.ToString)
                Using cmd As SqlCommand = cnxSource.CreateCommand
                    cnxSource.Open()
                    ' Requete ~= 12 secondes
                    cmd.CommandText = String.Format(
                        "SELECT {0}
                        FROM {1}.[FAMCHKSYNCVIEW] L 
                        LEFT JOIN {2}.[FAMCHKSYNCVIEW] R ON L.FA_CodeFamille = R.FA_CodeFamille
                        WHERE L.CS <> R.CS Or R.FA_CodeFamille Is NULL",
                        GetViewSelectAliases(vCols),
                        dbSource.sqlDbName,
                        dbTarget.sqlDbName                        
                    )
                    cmd.CommandTimeout = cmdTimeOut

                    Using reader As SqlDataReader = cmd.ExecuteReader
                        If reader.HasRows Then
                            While reader.Read
                                checkFamille(reader)
                            End While
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Log(ex.ToString)
        End Try
    End Sub

    'Champs à renseigner obligatoirement lors de l’ajout
    'FA_CodeFamille
    'FA_Intitule
    'FA_UniteVen
    '
    'Champs non modifiables en modification d’enregistrement
    'Pour tous les types :
    'FA_CodeFamille
    'FA_Type
    '
    'Si FA_Type = 1 ou 2 (Centralisateur ou Total) alors tous les champs sont non modifiables, sauf l'intitule
    Private Function checkFamille(ByRef viewFam As SqlDataReader) As Boolean
        Dim codeFam As String = viewFam.Item("L_FA_CodeFamille")
        Dim insert As Boolean = viewFam.Item("R_CS") = 0
        Log(String.Format("[{0}] {1}", codeFam, If(insert, "INSERT", "UPDATE")))

        Using cnxTarget As New SqlConnection(dbTarget.ToString)
            cnxTarget.Open()
            Dim transaction As SqlTransaction = cnxTarget.BeginTransaction

            Try
                Using cnxSource As New SqlConnection(dbSource.ToString)
                    cnxSource.Open()

                    '
                    ' F_FAMILLE
                    '
                    If viewFam.Item("L_csFamille").ToString <> viewFam.Item("R_csFamille").ToString Then
                        Using cmdSource As SqlCommand = GetSelectCmdFromCnx(
                            cnxSource,
                            "F_FAMILLE",
                            New List(Of String) From {"FA_CodeFamille", "FA_Type", "FA_Intitule", "FA_UniteVen", "FA_Coef", "FA_SuiviStock", "FA_Garantie", "FA_Central", "FA_CodeFiscal", "FA_Pays",
                                "FA_UnitePoids", "FA_Escompte", "FA_Delai", "FA_HorsStat", "FA_VteDebit", "FA_NotImp", "FA_Frais01FR_Denomination", "FA_Frais01FR_Rem01REM_Valeur",
                                "FA_Frais01FR_Rem01REM_Type", "FA_Frais01FR_Rem02REM_Valeur", "FA_Frais01FR_Rem02REM_Type", "FA_Frais01FR_Rem03REM_Valeur", "FA_Frais01FR_Rem03REM_Type",
                                "FA_Frais02FR_Denomination", "FA_Frais02FR_Rem01REM_Valeur", "FA_Frais02FR_Rem01REM_Type", "FA_Frais02FR_Rem02REM_Valeur", "FA_Frais02FR_Rem02REM_Type",
                                "FA_Frais02FR_Rem03REM_Valeur", "FA_Frais02FR_Rem03REM_Type", "FA_Frais03FR_Denomination", "FA_Frais03FR_Rem01REM_Valeur", "FA_Frais03FR_Rem01REM_Type",
                                "FA_Frais03FR_Rem02REM_Valeur", "FA_Frais03FR_Rem02REM_Type", "FA_Frais03FR_Rem03REM_Valeur", "FA_Frais03FR_Rem03REM_Type", "FA_Contremarque", "FA_FactPoids",
                                "FA_FactForfait", "FA_Publie", "FA_RacineRef", "FA_RacineCB", "CL_No1", "CL_No2", "CL_No3", "CL_No4", "FA_Nature", "FA_NbColis", "FA_SousTraitance", "FA_Fictif"},
                            codeFam
                            )

                            Using reader As SqlDataReader = cmdSource.ExecuteReader(CommandBehavior.SingleRow)
                                While reader.Read
                                    If insert Then
                                        InsertRowFromReader("F_FAMILLE", reader, transaction)
                                    Else
                                        ' Update
                                        Using cmdTarget As SqlCommand = cnxTarget.CreateCommand
                                            With cmdTarget
                                                .Transaction = transaction
                                                .CommandText = String.Format("UPDATE {0}.[dbo].[F_FAMILLE] SET ", cnxTarget.Database)

                                                Select Case reader.Item("FA_Type")
                                                    Case 0
                                                        For i As Integer = 0 To reader.FieldCount - 1
                                                            If reader.GetName(i) = "FA_Type" Or reader.GetName(i) = "FA_CodeFamille" Then
                                                                Continue For
                                                            End If

                                                            .CommandText &= String.Format("[{0}] = @{0}", reader.GetName(i))
                                                            .Parameters.AddWithValue(reader.GetName(i), reader.Item(i))

                                                            If i <> reader.FieldCount - 1 Then
                                                                .CommandText &= ", "
                                                            End If
                                                        Next
                                                    Case 1, 2
                                                        .CommandText &= "[FA_Intitule] = @faIntitule"
                                                        .Parameters.AddWithValue("@faIntitule", reader.Item("FA_Intitule"))
                                                    Case Else
                                                        Throw New Exception("FA_Type non pris en charge")
                                                End Select
                                                .CommandText &= " WHERE [FA_CodeFamille] = @codeFam"
                                                .Parameters.AddWithValue("@codeFam", codeFam)
                                                .CommandTimeout = cmdTimeOut
                                                .ExecuteNonQuery()
                                            End With
                                        End Using
                                    End If
                                End While
                            End Using
                        End Using
                    End If
                    '
                    ' END F_FAMILLE
                    '

                    '
                    ' F_FAMCOMPTA
                    '
                    If viewFam.Item("L_csFamComptaAgg").ToString <> viewFam.Item("R_csFamComptaAgg").ToString Then
                        Delete("F_FAMCOMPTA", transaction, "FA_CodeFamille", codeFam)
                        CopySourceToTarget(
                            cnxSource,
                            "F_FAMCOMPTA",
                            New List(Of String) From {"FA_CodeFamille", "FCP_Type", "FCP_Champ", "FCP_ComptaCPT_CompteG", "FCP_ComptaCPT_CompteA",
                                "FCP_ComptaCPT_Taxe1", "FCP_ComptaCPT_Taxe2", "FCP_ComptaCPT_Taxe3", "FCP_TypeFacture", "FCP_ComptaCPT_Date1",
                                "FCP_ComptaCPT_Date2", "FCP_ComptaCPT_Date3", "FCP_ComptaCPT_TaxeAnc1", "FCP_ComptaCPT_TaxeAnc2", "FCP_ComptaCPT_TaxeAnc3"},
                            codeFam,
                            transaction
                        )
                    End If

                    '
                    ' F_FAMFOURNISS
                    '
                    If viewFam.Item("L_csFamFournissAgg").ToString <> viewFam.Item("R_csFamFournissAgg").ToString Then
                        Delete("F_FAMFOURNISS", transaction, "FA_CodeFamille", codeFam)
                        CopySourceToTarget(
                            cnxSource,
                            "F_FAMFOURNISS",
                            New List(Of String) From {"FA_CodeFamille", "CT_Num", "FF_Unite", "FF_Conversion", "FF_DelaiAppro", "FF_Garantie", "FF_Colisage",
                                "FF_QteMini", "FF_QteMont", "EG_Champ", "FF_Principal", "FF_Devise", "FF_Remise", "FF_ConvDiv", "FF_TypeRem"},
                            codeFam,
                            transaction
                        )
                    End If

                    '
                    ' F_FAMTARIF
                    '
                    'Champs à renseigner obligatoirement lors de l’ajout
                    '            FA_CodeFamille
                    '            FT_Categorie
                    '            EG_Champ
                    'Champs non modifiables en modification d’enregistrement
                    '            EG_Champ
                    '            FA_CodeFamille
                    '            FT_Categorie
                    If viewFam.Item("L_csFamTarifAgg").ToString <> viewFam.Item("R_csFamTarifAgg").ToString Then
                        Delete("F_FAMTARIF", transaction, "FA_CodeFamille", codeFam)
                        CopySourceToTarget(
                            cnxSource,
                            "F_FAMTARIF",
                            New List(Of String) From {"FA_CodeFamille", "FT_Categorie", "FT_Coef", "FT_PrixTTC", "FT_Arrondi", "FT_QteMont", "EG_Champ", "FT_Devise", "FT_Remise", "FT_Calcul", "FT_TypeRem"},
                            codeFam,
                            transaction
                        )
                    End If

                    '
                    ' F_FAMTARIFQTE
                    '
                    'Champs à renseigner obligatoirement lors de l’ajout
                    '            FA_CodeFamille
                    '            FQ_RefCF
                    '            FQ_BorneSup
                    'Champs non modifiables en modification d’enregistrement
                    '            FA_CodeFamille
                    '            FQ_RefCF
                    '            FQ_BorneSup
                    If viewFam.Item("L_csFamTarifQteAgg").ToString <> viewFam.Item("R_csFamTarifQteAgg").ToString Then
                        Delete("F_FAMTARIFQTE", transaction, "FA_CodeFamille", codeFam)
                        CopySourceToTarget(
                            cnxSource,
                            "F_FAMTARIFQTE",
                            New List(Of String) From {"FA_CodeFamille", "FQ_RefCF", "FQ_BorneSup", "FQ_Remise01REM_Valeur", "FQ_Remise01REM_Type",
                                "FQ_Remise02REM_Valeur", "FQ_Remise02REM_Type", "FQ_Remise03REM_Valeur", "FQ_Remise03REM_Type"},
                            codeFam,
                            transaction
                        )
                    End If
                End Using ' End cnxSource

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

    ' Supprime les familles éventuelles
    Public Function DeleteFamille()
        ' TODO Ne doit fonctionner que dans un sens pour le moment
        If dbTarget.dbName Like "TARIF*" Then
            Log("Pas de delete famille sur une base tarif")
            Return True
        End If
        Log("[DEL FAMILLE]")

        Using cnxSource As New SqlConnection(dbTarget.ToString)
            Try
                cnxSource.Open()
                Dim tables As New List(Of String) From {"F_FAMCLIENT", "F_FAMCOMPTA", "F_FAMFOURNISS", "F_FAMMODELE", "F_FAMTARIF", "F_FAMTARIFQTE", "F_FAMILLE"}

                For Each table As String In tables
                    With cnxSource.CreateCommand
                        '.Transaction = transaction
                        .CommandText = String.Format(
                            "DELETE L FROM {0}.[{1}] L WHERE L.FA_CodeFamille NOT IN(SELECT R.FA_CodeFamille FROM {2}.[F_FAMILLE] R)",
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
            Finally
                cnxSource.Close()
            End Try
        End Using
    End Function
End Class
