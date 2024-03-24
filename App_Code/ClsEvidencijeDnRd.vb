Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsEvidencijeDnRd

    Public Sub transferEvid(ByVal pEmployeeID As Integer, ByVal pYear As Integer, ByVal pMonth As Integer)

        Dim mycmd As MySql.Data.MySqlClient.MySqlCommand

        Dim strSQL As String = <![CDATA[ 
 CALL EMP_evid_transfer( @pEmployeeID, @pYear, @pMonth);
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pEmployeeId", pEmployeeID)
            mycmd.Parameters.AddWithValue("@pYear", pYear)
            mycmd.Parameters.AddWithValue("@pMonth", pMonth)
            mycmd.Prepare()

            mycmd.ExecuteNonQuery()

        End Using

        transferEvidGO(pEmployeeID, pYear, pMonth)

    End Sub

    Private Sub transferEvidGO(ByVal pEmployeeID As Integer, ByVal pYear As Integer, ByVal pMonth As Integer)

        Dim mycmd As MySql.Data.MySqlClient.MySqlCommand

        Dim strSQL As String = <![CDATA[ 
 CALL EMP_evid_transferGO( @pEmployeeID, @pYear, @pMonth);
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pEmployeeId", pEmployeeID)
            mycmd.Parameters.AddWithValue("@pYear", pYear)
            mycmd.Parameters.AddWithValue("@pMonth", pMonth)

            mycmd.Prepare()

            mycmd.ExecuteNonQuery()

        End Using
    End Sub

    Public Sub transferEvidDel(ByVal pEmployeeID As Integer, ByVal pYear As Integer, ByVal pMonth As Integer)

        Dim mycmd As MySql.Data.MySqlClient.MySqlCommand

        Dim strSQL As String = <![CDATA[ 
 CALL EMP_evid_transfer_del( @pEmployeeID, @pYear, @pMonth);
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pEmployeeId", pEmployeeID)
            mycmd.Parameters.AddWithValue("@pYear", pYear)
            mycmd.Parameters.AddWithValue("@pMonth", pMonth)
            mycmd.Prepare()

            mycmd.ExecuteNonQuery()

        End Using
    End Sub

    Public Sub getGOTotals(ByVal pEmployeeId As Integer, ByVal pYear As Integer, ByRef pSGOUk As Integer, ByRef pSGOIsk As Integer, ByRef pNGOUk As Integer, ByRef pNGOIsk As Integer)

        pSGOUk = 0
        pSGOIsk = 0
        pNGOUk = 0
        pNGOIsk = 0

        Dim strSQL As String

        strSQL = <sql text="SELECT ego.`IzrDani` + ego.`IzrDaniPreneseni`-ego.`IzrDaniAnul` AS IzrDani FROM `evd_godisnjiodmor` ego 
                            WHERE ego.`Godina`=@pYear AND ego.EmployeeID=@pEmployeeID;"/>.Attribute("text").Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlClient.MySqlDataReader
            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL

            ''
            ' GO tekuće godine
            ''
            mycmd.Parameters.AddWithValue("@pEmployeeId", pEmployeeId)
            mycmd.Parameters.AddWithValue("@pYear", pYear)
            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader
            Do While dbRead.Read
                pNGOUk = dbRead.GetInt32("IzrDani").ToString
            Loop
            dbRead.Close()

            mycmd.Parameters.Clear()
            ''
            ' GO prošle godine
            ''

            mycmd.Parameters.AddWithValue("@pEmployeeId", pEmployeeId)
            mycmd.Parameters.AddWithValue("@pYear", pYear - 1)

            dbRead = mycmd.ExecuteReader
            Do While dbRead.Read
                pSGOUk = dbRead.GetInt32("IzrDani").ToString
            Loop
            dbRead.Close()

            ''
            ' Iskorišten GO tekuće godine
            ''

            strSQL = <sql text="SELECT COUNT(ep.`vrijeme_ukupno`) AS NumOfDays FROM `evd_prisustva` ep WHERE ep.`employeeID`=@pEmployeeID AND 
                ep.`sifra_placanja`=@pSifra GROUP BY ep.`sifra_placanja`;"/>.Attribute("text").Value
            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@pEmployeeId", pEmployeeId)
            mycmd.Parameters.AddWithValue("@pSifra", "GO-" + pYear.ToString)

            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader
            Do While dbRead.Read
                pNGOIsk = dbRead.GetInt32("NumOfDays").ToString
            Loop
            dbRead.Close()

            ''
            ' Iskorišten GO prošle godine
            ''
            mycmd.Parameters.Clear()

            mycmd.Parameters.AddWithValue("@pEmployeeId", pEmployeeId)
            mycmd.Parameters.AddWithValue("@pSifra", "GO-" + (pYear - 1).ToString)

            dbRead = mycmd.ExecuteReader
            Do While dbRead.Read
                pSGOIsk = dbRead.GetInt32("NumOfDays").ToString
            Loop
            dbRead.Close()

        End Using

    End Sub

    Public Sub getGOTotalsRjes(ByVal pEmployeeId As Integer, ByVal pYear As Integer, ByVal pMonth As Integer, ByRef pGORjes1 As Integer, ByRef pGORjes2 As Integer)

        pGORjes1 = 0
        pGORjes2 = 0


        Dim strSQL As String = <![CDATA[ 
SELECT COUNT(sc.dt) AS sumaGO, ep.`employeeID` FROM `evd_godisnjiodmor_plan` ep
INNER JOIN `evd_godisnjiodmor` ego ON ep.godina=ego.Godina AND ego.employeeID=ep.employeeID, 
 `sfr_calendar` sc, evd_employees e 
WHERE (ep.`startDate`<=sc.`dt` AND ep.`endDate`>=sc.dt) AND ep.`period`=1 AND sc.`isWeekday`=1 AND sc.`isHoliday`=0
AND MONTH(sc.dt)=@pMonth AND YEAR(sc.dt)=@pYear AND e.`EmployeeID`=ep.`employeeID`  AND ep.`brojDana`>0
AND ego.rjesenje_status=-1 AND  e.`EmployeeID`=@pEmployeeId 
GROUP BY ep.`employeeID`;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlClient.MySqlDataReader
            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL

            ''
            ' GO tekuće godine
            ''
            mycmd.Parameters.AddWithValue("@pEmployeeId", pEmployeeId)
            mycmd.Parameters.AddWithValue("@pYear", pYear)
            mycmd.Parameters.AddWithValue("@pMonth", pMonth)

            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader
            Do While dbRead.Read
                pGORjes1 = dbRead.GetInt32("sumaGO").ToString
            Loop
            dbRead.Close()

            mycmd.Parameters.Clear()
            ''
            ' GO termin 2
            ''
            strSQL = <![CDATA[ 
SELECT COUNT(sc.dt) AS sumaGO, ep.`employeeID` FROM `evd_godisnjiodmor_plan` ep
INNER JOIN `evd_godisnjiodmor` ego ON ep.godina=ego.Godina AND ego.employeeID=ep.employeeID, 
 `sfr_calendar` sc, evd_employees e 
WHERE (ep.`startDate`<=sc.`dt` AND ep.`endDate`>=sc.dt) AND ep.`period`=2 AND sc.`isWeekday`=1 AND sc.`isHoliday`=0
AND MONTH(sc.dt)=@pMonth AND YEAR(sc.dt)=@pYear AND e.`EmployeeID`=ep.`employeeID`  AND ep.`brojDana`>0
AND ego.rjesenje_status=-1 AND  e.`EmployeeID`=@pEmployeeId 
GROUP BY ep.`employeeID`;
    ]]>.Value

            mycmd.CommandText = strSQL

            ''
            ' GO tekuće godine
            ''
            mycmd.Parameters.AddWithValue("@pEmployeeId", pEmployeeId)
            mycmd.Parameters.AddWithValue("@pYear", pYear)
            mycmd.Parameters.AddWithValue("@pMonth", pMonth)

            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader
            Do While dbRead.Read
                pGORjes2 = dbRead.GetInt32("sumaGO").ToString
            Loop
            dbRead.Close()

        End Using

    End Sub

End Class
