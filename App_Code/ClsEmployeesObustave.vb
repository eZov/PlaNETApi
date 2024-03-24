Imports System.Collections.Generic
Imports System.Data
Imports System.Web.Configuration
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsEmployeesObustave

    Public Sub New()

        DtList = New DataTable

    End Sub


    Public Property DtList As DataTable

    Public Function getList(ByVal pSessionId As String, ByVal pOrgSfr As String, Optional ByVal pEmpAll As Boolean = False) As DataTable

        Dim myda As MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String


            strSQL = <![CDATA[ 
CALL EVDwb_select_uposleniobustave(@SessionId,@OrgSfr, FALSE);
]]>.Value
            If pEmpAll Then
                strSQL = <![CDATA[ 
CALL EVDwb_select_uposleniobustave(@SessionId,@OrgSfr, TRUE);
]]>.Value
            End If


            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@SessionId", pSessionId)
            mycmd.Parameters.AddWithValue("@OrgSfr", pOrgSfr)
            mycmd.Prepare()

            DtList.Rows.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)

        End Using


        Return DtList



    End Function

    '    Public Sub addEmpObustave(ByRef pFields As Dictionary(Of String, Object))

    '        Dim mycmd As MySqlCommand

    '        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
    '            myconnection.Open()

    '            mycmd = New MySqlCommand
    '            mycmd.Connection = myconnection

    '            Dim strSQL As String = <![CDATA[ 
    'INSERT INTO `evd_uposleniobustave` (idEmployee, idObustave)
    'VALUES (@idEmployee, @idObustave);
    ']]>.Value
    '            mycmd.CommandText = strSQL
    '            mycmd.Prepare()


    '            Dim rowKey1 As Integer = CInt(pFields("idEmployee"))
    '            Dim rowKey2 As Integer = CInt(pFields("idObustave"))



    '            Try
    '                mycmd.Parameters.Clear()
    '                mycmd.Parameters.AddWithValue("@idEmployee", rowKey1)
    '                mycmd.Parameters.AddWithValue("@idObustave", rowKey2)
    '                mycmd.ExecuteNonQuery()
    '            Catch ex As MySqlException

    '            End Try





    '        End Using


    '    End Sub

    '    Public Sub updateEmpObustave(ByRef pRows As ArrayList)

    '        Dim mycmd As MySqlCommand

    '        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
    '            myconnection.Open()

    '            mycmd = New MySqlCommand
    '            mycmd.Connection = myconnection

    '            Dim strSQL As String = <![CDATA[ 
    'UPDATE `evd_uposleniobustave`
    'SET
    '  PartijaKred = @PartijaKred, 
    '  Procenat = @Procenat, 
    '  Zaduzenje = @Zaduzenje, 
    '  IznosRate = @IznosRate, 
    '  BrojRata = @BrojRata
    'WHERE `id` = @id AND  Aktivno<>0 AND Completed = 0;
    ']]>.Value
    '            mycmd.CommandText = strSQL
    '            mycmd.Prepare()

    '            For Each el As Dictionary(Of String, Object) In pRows
    '                Dim rowKey As Integer = CInt(el("id"))
    '                Dim rowPartijaKred As String = CStr(el("PartijaKred"))
    '                Dim rowProcenat As Double = CDbl(el("Procenat"))
    '                Dim rowZaduzenje As Double = CDbl(el("Zaduzenje"))
    '                Dim rowIznosRate As Double = CDbl(el("IznosRate"))
    '                Dim rowBrojRata As Double = CDbl(el("BrojRata"))

    '                If rowKey > 0 Then

    '                    Try
    '                        mycmd.Parameters.Clear()
    '                        mycmd.Parameters.AddWithValue("@id", rowKey)
    '                        mycmd.Parameters.AddWithValue("@PartijaKred", rowPartijaKred)
    '                        mycmd.Parameters.AddWithValue("@Procenat", rowProcenat)
    '                        mycmd.Parameters.AddWithValue("@Zaduzenje", rowZaduzenje)
    '                        mycmd.Parameters.AddWithValue("@IznosRate", rowIznosRate)
    '                        mycmd.Parameters.AddWithValue("@BrojRata", rowBrojRata)

    '                        mycmd.ExecuteNonQuery()
    '                    Catch ex As MySqlException

    '                    End Try

    '                End If

    '            Next

    '        End Using


    '    End Sub

    '    Public Sub deleteEmpObustave(ByRef pRows As ArrayList)

    '        Dim mycmd As MySqlCommand

    '        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
    '            myconnection.Open()

    '            mycmd = New MySqlCommand
    '            mycmd.Connection = myconnection

    '            Dim strSQL As String = <![CDATA[ 
    'UPDATE `evd_uposleniobustave`
    'SET
    ' Aktivno = 0, 
    ' Completed = 1
    'WHERE `id` = @id;
    ']]>.Value
    '            mycmd.CommandText = strSQL
    '            mycmd.Prepare()

    '            For Each el As Dictionary(Of String, Object) In pRows
    '                Dim rowKey As Integer = CInt(el("id"))

    '                If rowKey > 0 Then

    '                    Try
    '                        mycmd.Parameters.Clear()
    '                        mycmd.Parameters.AddWithValue("@id", rowKey)
    '                        mycmd.ExecuteNonQuery()
    '                    Catch ex As MySqlException

    '                    End Try

    '                End If

    '            Next

    '        End Using


    '    End Sub

    Public Sub deleteEmpObustave()

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
UPDATE `evd_uposleniobustave`
SET
 Completed = -1
WHERE Aktivno = 0;
]]>.Value
            mycmd.CommandText = strSQL

            'mycmd.Parameters.Clear()
            'mycmd.Parameters.AddWithValue("@id", rowKey)
            mycmd.ExecuteNonQuery()

        End Using


    End Sub

End Class
