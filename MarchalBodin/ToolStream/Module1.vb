Imports Objets100Lib
Imports Excel = Microsoft.Office.Interop.Excel
Imports System.IO
Imports System.Data
Imports System.Data.SqlClient

Module Module1
    Private OM_BaseCial As New BSCIALApplication3
    Private cnx As New SqlClient.SqlConnection
    Private ExcelApp As New Excel.Application

    Sub Main()
        Try
            If Environment.GetCommandLineArgs.Length <> 4 Then
                Throw New Exception("Nombre d'arguments invalide !")
            Else

                OM_BaseCial.Name = Environment.GetCommandLineArgs.GetValue(1)
                OM_BaseCial.Loggable.UserName = Environment.GetCommandLineArgs.GetValue(2)
                OM_BaseCial.Loggable.UserPwd = Environment.GetCommandLineArgs.GetValue(3)
                OM_BaseCial.Open()

                cnx.ConnectionString = String.Format("server={0};Trusted_Connection=yes;database={1};MultipleActiveResultSets=True", OM_BaseCial.DatabaseInfo.ServerName, OM_BaseCial.DatabaseInfo.DatabaseName)
                cnx.Open()

                Dim cmd As SqlCommand = cnx.CreateCommand
                cmd.CommandText = "SELECT TOP 1 AR_Ref FROM F_ARTFOURNISS WHERE CT_Num = 'FTO98800' ORDER BY AR_Ref DESC"
                Dim reader As SqlDataReader = cmd.ExecuteReader(CommandBehavior.SingleResult)

                Dim refMagBase As Integer
                While reader.Read
                    refMagBase = Split(reader.Item("AR_Ref"), "-").Last
                End While
                refMagBase += 3

                reader.Close()
                cmd.Dispose()

                Dim cdesBook As Excel.Workbook = ExcelApp.Workbooks.Open("S:\DIVERS\Toolstream_Francois.xlsx")
                Dim tarifBook As Excel.Workbook = ExcelApp.Workbooks.Open("S:\FOURNISSEURS\Toolstream\2017 S2\Liste Excel de prix fin 2017 et bar codes en francais.xls")
                Dim cdesSheet As Excel.Worksheet = cdesBook.Worksheets("Feuil1")
                Dim tarifSheet As Excel.Worksheet = tarifBook.Worksheets("Produit")
                Dim cdesRc As Integer = cdesSheet.UsedRange.Rows.Count
                Dim tarifRc As Integer = tarifSheet.UsedRange.Rows.Count

                Dim fourn As IBOFournisseur3 = OM_BaseCial.CptaApplication.FactoryFournisseur.ReadNumero("FTO98800")
                Dim famF As IBOFamille3 = OM_BaseCial.FactoryFamille.ReadCode(FamilleType.FamilleTypeDetail, "F")
                Dim unite As IBPUnite = OM_BaseCial.FactoryUnite.ReadIntitule("PCE")
                'Dim gammeRemise As IBPGamme = OM_BaseCial.FactoryGamme.ReadIntitule("quantite")
                'Dim refMagBase As Integer = 553

                For i As Integer = 2 To cdesRc
                    Dim cRef As String = cdesSheet.Cells(i, 1).Value

                    If cRef = "" Then
                        Exit For
                    End If

                    Dim coef As Double = cdesSheet.Cells(i, 2).Value

                    'For ii As Integer = 1 To tarifRc
                    '    Dim tRef As String = tarifSheet.Cells(i, 1).Value
                    '    If cRef = tRef Then
                    '        Dim des As String = tarifSheet.Cells(i, 5).Value
                    '        Debug.Print(des)
                    '    End If
                    'Next

                    Dim currentFind As Excel.Range = Nothing
                    Dim firstFind As Excel.Range = Nothing

                    'Debug.Print(cRef)
                    Console.WriteLine(cRef)
                    currentFind = tarifSheet.UsedRange.Columns(1).Find(cRef)
                    If currentFind Is Nothing Then
                        'Debug.Print("Non trouvé")
                        Console.WriteLine("Non trouvé")
                    End If
                    While Not currentFind Is Nothing
                        If firstFind Is Nothing Then
                            firstFind = currentFind
                            Dim des As String = Left(Trim(String.Format("{0} {1}", tarifSheet.Cells(firstFind.Row, 5).Value, tarifSheet.Cells(firstFind.Row, 6).Value)), 69).ToUpper

                            Dim colisage As Integer = tarifSheet.Cells(firstFind.Row, 11).Value
                            Dim prix As Double = tarifSheet.Cells(firstFind.Row, 12).Value

                            Dim qec As Integer = tarifSheet.Cells(firstFind.Row, 13).Value
                            Dim prixQec As Double = tarifSheet.Cells(firstFind.Row, 14).Value

                            Dim gencode As String = tarifSheet.Cells(firstFind.Row, 15).Value

                            cmd = cnx.CreateCommand
                            cmd.CommandText = "SELECT AR_Ref FROM F_ARTFOURNISS WHERE AF_RefFourniss = @ref AND CT_Num = @ctnum"
                            cmd.Parameters.AddWithValue("@ref", cRef)
                            cmd.Parameters.AddWithValue("@ctnum", fourn.CT_Num)
                            reader = cmd.ExecuteReader

                            If reader.HasRows = False Then
                                Dim article As IBOArticle3 = OM_BaseCial.FactoryArticle.Create
                                article.Famille = famF
                                article.AR_Ref = String.Format("988-0{0}", refMagBase)
                                article.AR_Design = des
                                article.Unite = unite
                                article.AR_PrixAchat = prix
                                article.AR_Coef = coef
                                article.AR_PrixVen = prix * coef
                                article.SetDefault()
                                article.AR_SuiviStock = SuiviStockType.SuiviStockTypeCmup
                                article.WriteDefault()

                                article.InfoLibre.Item("ETIQUETTE_FORMAT") = "FORMAT_1"
                                article.Write()
                                '
                                '''' PX ACH, COEF, PX VEN + CAT TARIFS
                                '
                                For Each c As IBOArticleTarifCategorie3 In article.FactoryArticleTarifCategorie.List
                                    c.Remove()
                                Next

                                For Each catTarif As IBOFamilleTarifCategorie In famF.FactoryFamilleTarifCategorie.List
                                    Dim c As IBOArticleTarifCategorie3 = article.FactoryArticleTarifCategorie.Create()
                                    c.Article = article
                                    c.PrixTTC = catTarif.PrixTTC
                                    c.CategorieTarif = catTarif.CategorieTarif

                                    If catTarif.PrixTTC = True Then
                                        c.Prix = Math.Round(article.AR_PrixVen * 1.2, 2)
                                    End If

                                    c.Remise = catTarif.Remise
                                    c.WriteDefault()
                                Next

                                Dim tarif As IBOArticleTarifFournisseur3 = fourn.FactoryFournisseurTarif.Create()
                                tarif.Article = article
                                tarif.Reference = cRef
                                tarif.AF_CodeBarre = gencode
                                tarif.AF_Colisage = colisage
                                tarif.AF_QteMini = qec
                                tarif.Prix = prix
                                tarif.Unite = unite
                                'tarif.GammeRemise = gammeRemise
                                tarif.WriteDefault()

                                'If qec > 1 Then
                                '    Dim gm1 As IBOArticleTarifQteFournisseur3 = tarif.FactoryArticleTarifQte.Create
                                '    gm1.BorneSup = qec - 1
                                '    gm1.WriteDefault()

                                '    Dim gm2 As IBOArticleTarifQteFournisseur3 = tarif.FactoryArticleTarifQte.Create
                                '    gm2.BorneSup = 99999999999999
                                '    gm2.Remise.FromString(String.Format("{0}U", prix - prixQec))
                                '    gm2.WriteDefault()
                                'End If

                                refMagBase += 3
                            Else
                                Console.WriteLine("L'article existe deja {0}", cRef)
                                While reader.Read
                                    Dim article As IBOArticle3 = OM_BaseCial.FactoryArticle.ReadReference(reader.Item("AR_Ref"))
                                    article.AR_Design = des
                                    article.AR_PrixAchat = prix
                                    article.AR_Coef = coef
                                    article.AR_PrixVen = prix * coef

                                    article.Write()

                                    Dim tarif As IBOArticleTarifFournisseur3 = fourn.FactoryFournisseurTarif.ReadArticle(article)
                                    tarif.AF_CodeBarre = gencode
                                    tarif.AF_Colisage = colisage
                                    tarif.AF_QteMini = qec
                                    tarif.Prix = prix
                                    tarif.Unite = unite
                                    tarif.Write()
                                End While
                            End If

                            Console.WriteLine("{0} {1} {2} {3} {4} {5}", cRef, des, colisage, prix, qec, gencode)

                            Exit While
                        End If

                        currentFind = tarifSheet.Next(currentFind)
                    End While

                Next
                cdesBook.Close(False)
                tarifBook.Close(False)
            End If

        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
        Console.WriteLine("Fin du process...")
        Console.Read()
    End Sub

End Module
