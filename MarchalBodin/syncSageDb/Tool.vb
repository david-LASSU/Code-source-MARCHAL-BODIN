Imports System.Data.SqlClient
Imports System.Security.Cryptography
Imports System.Text

Public MustInherit Class Tool
    Protected dbSource As Database
    Protected dbTarget As Database

    Public Sub New(ByRef dbTarget As Database, Optional ByRef dbSource As Database = Nothing)
        Me.dbTarget = dbTarget
        Me.dbSource = dbSource
    End Sub

    Public Sub Log(logMessage As String)
        Module1.Log(logMessage, dbTarget)
    End Sub

    ' Automate the aliases of the view select
    Protected Function GetViewSelectAliases(ByRef vCols As List(Of String)) As String
        Dim str As New List(Of String)
        For Each c As String In vCols
            str.Add(String.Format("L.{0} AS L_{0}, ISNULL(R.{0}, 0) AS R_{0} ", c))
        Next
        Return String.Join(",", str.ToArray)
    End Function

    Protected Sub InsertRowFromReader(ByVal table As String, ByRef reader As SqlDataReader, ByRef transaction As SqlTransaction)
        Dim cnxTarget As SqlConnection = transaction.Connection
        Dim cleanInfoName As String
        Dim logString As String = String.Format("INSERT INTO {0}.[dbo].[{1}]", cnxTarget.Database, table)

        Using cmdTarget As SqlCommand = cnxTarget.CreateCommand
            With cmdTarget
                .Transaction = transaction

                Dim fieldStr As String = ""
                Dim valueStr As String = ""

                For i As Integer = 0 To reader.FieldCount - 1
                    If i > 0 Then
                        fieldStr &= ", "
                        valueStr &= ", "
                    End If

                    cleanInfoName = reader.GetName(i).Replace(" ", "").Replace("°", "")
                    fieldStr &= String.Format("[{0}]", reader.GetName(i))
                    valueStr &= String.Format("@{0}", cleanInfoName)
                    .Parameters.AddWithValue(cleanInfoName, reader.Item(i))

                    logString &= String.Format(" [{0} = {1}]", reader.GetName(i), reader.Item(i))
                Next
                .CommandText = String.Format("INSERT INTO {0}.[dbo].[{1}] ({2}) VALUES ({3})", cnxTarget.Database, table, fieldStr, valueStr)
                .CommandTimeout = cmdTimeOut
                Log(logString)

                .ExecuteNonQuery()
            End With
        End Using
    End Sub

    ''' <summary>
    ''' Construit une commande de table select d'une table source
    ''' en utilisant une liste de champs dont le premier élément est la clé primaire
    ''' </summary>
    ''' <param name="cnx"></param>
    ''' <param name="table"></param>
    ''' <param name="fields"></param>
    ''' <param name="pkValue"></param>
    ''' <param name="andPart"></param>
    ''' <returns></returns>
    Protected Function GetSelectCmdFromCnx(ByRef cnx As SqlConnection, ByVal table As String, ByVal fields As List(Of String), pkValue As String, Optional ByVal andPart As String = "") As SqlCommand
        Dim cmd As SqlCommand = cnx.CreateCommand
        With cmd
            .CommandText = String.Format(
                                "SELECT [{0}] FROM [{1}].[{2}].[dbo].[{3}] WHERE [{4}] = @value {5}",
                                String.Join("],[", fields.ToArray),
                                cnx.DataSource,
                                cnx.Database,
                                table,
                                fields.First,
                                If(andPart <> "", "AND " & andPart, "")
                            )
            .Parameters.AddWithValue("@value", pkValue)
            .CommandTimeout = cmdTimeOut
        End With

        Return cmd
    End Function

    ''' <summary>
    ''' Automatise le process de copier les données d'une table source vers une table destination
    ''' </summary>
    ''' <param name="table"></param>
    ''' <param name="fields"></param>
    ''' <param name="pkValue"></param>
    ''' <param name="targetTransaction"></param>
    ''' <param name="andPart"></param>
    Protected Sub CopySourceToTarget(ByRef cnx As SqlConnection, ByVal table As String, ByVal fields As List(Of String), pkValue As String, ByRef targetTransaction As SqlTransaction, Optional ByVal andPart As String = "")

            Using cmdSource As SqlCommand = GetSelectCmdFromCnx(cnx, table, fields, pkValue, andPart)
                'Log(cmdSource.CommandText)
                Using reader As SqlDataReader = cmdSource.ExecuteReader
                    If reader.HasRows Then
                        While reader.Read
                            InsertRowFromReader(table, reader, targetTransaction)
                        End While
                    End If
                End Using
            End Using
    End Sub

    ''' <summary>
    ''' Delete from table where pKey = value
    ''' </summary>
    ''' <param name="table"></param>
    ''' <param name="transaction"></param>
    ''' <param name="pKey"></param>
    ''' <param name="value"></param>
    Protected Sub Delete(ByVal table As String, ByRef transaction As SqlTransaction, ByVal pKey As String, ByVal value As String, Optional ByVal andPart As String = "")
        Dim cnxTarget As SqlConnection = transaction.Connection

        Log(String.Format("DELETE FROM {0}.[dbo].[{1}] WHERE {2} = {3} {4}", cnxTarget.Database, table, pKey, value, If(andPart <> "", "AND " & andPart, "")))

        Using cmd As SqlCommand = cnxTarget.CreateCommand
            With cmd
                .Transaction = transaction
                .CommandText = String.Format("DELETE FROM {0}.[dbo].[{1}] WHERE {2} = @value {3}", cnxTarget.Database, table, pKey, If(andPart <> "", "AND " & andPart, ""))
                .Parameters.AddWithValue("@value", value)
                .CommandTimeout = cmdTimeOut
                .ExecuteNonQuery()
            End With
        End Using
    End Sub

    Protected Function GetStringFromHash(ByVal value As Object) As String
        If IsDBNull(value) Then
            Return String.Empty
        End If

        using hasher As MD5 = MD5.Create
            Dim dbytes As Byte() = hasher.ComputeHash(value)
            Dim sBuilder As New StringBuilder()
            For n As Integer = 0 To dbytes.Length - 1
                sBuilder.Append(dbytes(n).ToString("X2"))
            Next n

            Return sBuilder.ToString
        End Using
    End Function

    Protected Function AreIdenticals(ByVal LVal As Object, ByVal RVal As Object) As Boolean
        Return GetStringFromHash(LVal).Equals(GetStringFromHash(RVal))
    End Function
End Class
