Imports System.Collections.Generic
Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsEmployeeObustave

    Public Sub New(ByVal pEmployeeId As Integer)

        DtList = New DataTable
        EmployeeId = pEmployeeId

    End Sub

    Public Sub New()

        DtList = New DataTable

    End Sub

    Public Property EmployeeId As Integer

    Public Property DtList As DataTable

    Public Function getList() As DataTable

        Dim myda As MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT uo.`id`,
eo.`Naziv`,
IF(IFNULL(eo.`PoslovniPartner`,-1)<>-1, pp.`Naziv`, b.`Naziv`) AS PoslPartner,
uo.`PartijaKred`, 
uo.`Zaduzenje`,
ROUND(uo.`IznosUplata`,2) AS IznosUplata,
IF(uo.`Zaduzenje`-uo.`IznosUplata`<0,0,ROUND(uo.`Zaduzenje`-uo.`IznosUplata`,2)) AS OstDuga, 
IF(uo.`Zaduzenje`-uo.`IznosUplata`<0,0,IFNULL(ROUND((uo.`Zaduzenje`-uo.`IznosUplata`)/uo.`IznosRate`,2),0)) AS OstRata, 
uo.`Procenat`, uo.`IznosRate`,
uo.`BrojRata`,
uo.Aktivno
FROM `evd_uposleniobustave` uo LEFT JOIN `evd_obustave` eo ON uo.`idObustave`=eo.`id`
LEFT JOIN `sfr_poslovnipartneri` pp ON eo.`PoslovniPartner`=pp.`id`
LEFT JOIN `sfr_banke` b ON eo.`Banka`=b.`id`
WHERE uo.`idEmployee`=@employeeID  AND  uo.Completed = 0 ORDER BY eo.`Naziv`;
]]>.Value
            'mycmd.CommandText = strSQL.Replace("@Sifra", pSfr)
            mycmd.CommandText = strSQL

            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employeeID", EmployeeId)
            mycmd.Prepare()

            DtList.Rows.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)

        End Using


        Return DtList

    End Function

    Public Sub addEmpObustave(ByRef pFields As Dictionary(Of String, Object))

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
INSERT INTO `evd_uposleniobustave` (idEmployee, idObustave)
VALUES (@idEmployee, @idObustave);
]]>.Value
            mycmd.CommandText = strSQL


            Dim rowKey1 As Integer = CInt(pFields("idEmployee"))
            Dim rowKey2 As Integer = CInt(pFields("idObustave"))



            Try
                mycmd.Parameters.Clear()
                mycmd.Parameters.AddWithValue("@idEmployee", rowKey1)
                mycmd.Parameters.AddWithValue("@idObustave", rowKey2)
                mycmd.Prepare()

                mycmd.ExecuteNonQuery()
            Catch ex As MySqlException

            End Try





        End Using


    End Sub

    Public Sub updateEmpObustave(ByRef pRows As ArrayList)

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
UPDATE `evd_uposleniobustave`
SET
  PartijaKred = @PartijaKred, 
  Procenat = @Procenat, 
  Zaduzenje = @Zaduzenje, 
  IznosRate = @IznosRate, 
  BrojRata = @BrojRata,
  Aktivno = @Aktivno
WHERE `id` = @id AND Completed = 0;
]]>.Value
            mycmd.CommandText = strSQL

            For Each el As Dictionary(Of String, Object) In pRows
                Dim rowKey As Integer = CInt(el("id"))
                Dim rowPartijaKred As String = CStr(el("PartijaKred"))
                Dim rowProcenat As Double = CDbl(el("Procenat"))
                Dim rowZaduzenje As Double = CDbl(el("Zaduzenje"))
                Dim rowIznosRate As Double = CDbl(el("IznosRate"))
                Dim rowBrojRata As Double = CDbl(el("BrojRata"))
                Dim rowAktivno As Integer = CInt(el("Aktivno"))

                If rowKey > 0 Then

                    Try
                        mycmd.Parameters.Clear()
                        mycmd.Parameters.AddWithValue("@id", rowKey)
                        mycmd.Parameters.AddWithValue("@PartijaKred", rowPartijaKred)
                        mycmd.Parameters.AddWithValue("@Procenat", rowProcenat)
                        mycmd.Parameters.AddWithValue("@Zaduzenje", rowZaduzenje)
                        mycmd.Parameters.AddWithValue("@IznosRate", rowIznosRate)
                        mycmd.Parameters.AddWithValue("@BrojRata", rowBrojRata)
                        mycmd.Parameters.AddWithValue("@Aktivno", rowAktivno)
                        mycmd.Prepare()

                        mycmd.ExecuteNonQuery()
                    Catch ex As MySqlException

                    End Try

                End If

            Next

        End Using


    End Sub

    Public Sub deleteEmpObustave(ByRef pRows As ArrayList)

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
UPDATE `evd_uposleniobustave`
SET
 Aktivno = 0, 
 Completed = 1
WHERE `id` = @id;
]]>.Value
            mycmd.CommandText = strSQL

            For Each el As Dictionary(Of String, Object) In pRows
                Dim rowKey As Integer = CInt(el("id"))

                If rowKey > 0 Then

                    Try
                        mycmd.Parameters.Clear()
                        mycmd.Parameters.AddWithValue("@id", rowKey)
                        mycmd.Prepare()

                        mycmd.ExecuteNonQuery()
                    Catch ex As MySqlException

                    End Try

                End If

            Next

        End Using


    End Sub


End Class
