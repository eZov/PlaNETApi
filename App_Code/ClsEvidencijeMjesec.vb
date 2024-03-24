Imports System.Data
Imports System.Globalization
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsEvidencijeMjesec

    Public Property CurrentEmployeeId As Integer
    Public CurrentEmployeeName As String

    Public Property Year As Integer
    Public Property Month As Integer

    Public Property TblEvidDani As DataTable

    Public Property Trajanje As Double
    Public Property SumeTrajanja As New Dictionary(Of String, Double)

    Public Property Podnio As Integer
    Public Property Odobrio As Integer
    Public Property Kontrolisao As Integer
    Public Property Locked As Boolean

    Public Property OdobError As Boolean

    Public Property EditEnabled As Boolean
    Public Property NoEdit As Boolean

    Public Property EmpPodnio As String
    Public Property EmpOdobrio As String
    Public Property EmpKontrolisao As String

    Public Sub New()

        TblEvidDani = New DataTable

        Podnio = -1
        Odobrio = -1
        Kontrolisao = -1
        Locked = -1

        OdobError = False

        EditEnabled = True
        NoEdit = False

        EmpPodnio = ""
        EmpOdobrio = ""
        EmpKontrolisao = ""


    End Sub

    Public Sub getEmpData()

        Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand
        Dim dbRead As MySqlClient.MySqlDataReader

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim strSQL As String = <![CDATA[ 
SELECT CONCAT( e.`LastName`,' ' ,e.`FirstName`) AS EmployeeName FROM `evd_employees` e
WHERE e.`EmployeeID`=@employeeID;
]]>.Value

            mycmd.Connection = myconnection
            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employeeID", CurrentEmployeeId)
            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader
            Do While dbRead.Read
                CurrentEmployeeName = dbRead.GetString("EmployeeName")

            Loop
            dbRead.Close()

        End Using


    End Sub

    Public Sub getMonth()

        Dim myda As MySqlClient.MySqlDataAdapter

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim strSQL As String = <![CDATA[ 
Select * from evd_prisustva where employeeID = ?employeeID AND YEAR(datum) = ?year AND MONTH(datum) = ?month;]]>.Value


            strSQL = strSQL.Replace("?employeeID", CurrentEmployeeId)
            strSQL = strSQL.Replace("?year", Year)
            strSQL = strSQL.Replace("?month", Month)


            TblEvidDani.Clear()

            myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(TblEvidDani)

        End Using

        getSumeTrajanja()
        'getOdobrenja()

    End Sub

    Private Sub getSumeTrajanja()

        Dim strSQL As String
        Dim dbRead As MySqlClient.MySqlDataReader
        Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd.Connection = myconnection


            strSQL = <![CDATA[ 
SELECT ep.`sifra_placanja`, SUM(ep.`vrijeme_ukupno`)/10000 AS sumaVrijeme 
FROM `evd_prisustva` ep 
WHERE ep.`employeeID`=@employeeID AND YEAR(ep.`datum`)=@Year AND MONTH(ep.`datum`)=@Month
GROUP BY ep.`sifra_placanja`;
]]>.Value
            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employeeID", CurrentEmployeeId)
            mycmd.Parameters.AddWithValue("@Year", Year)
            mycmd.Parameters.AddWithValue("@Month", Month)
            mycmd.Prepare()

            Dim _sifraPlacanja As String
            Dim _sumaVrijeme As Double
            Dim _sumaVrijemeUk As Double = 0

            dbRead = mycmd.ExecuteReader
            Do While dbRead.Read
                _sifraPlacanja = dbRead.GetString("sifra_placanja")
                _sumaVrijeme = dbRead.GetDouble("sumaVrijeme")
                SumeTrajanja.Add(_sifraPlacanja, _sumaVrijeme)
                _sumaVrijemeUk += _sumaVrijeme
            Loop
            SumeTrajanja.Add("UKUPNO", _sumaVrijemeUk)
            dbRead.Close()

        End Using


    End Sub

    Public Sub getOdobrenja(Optional ByVal pEmpRole As String = "uposlenik")

        Dim strSQL As String
        Dim dbRead As MySqlClient.MySqlDataReader
        Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd.Connection = myconnection


            strSQL = <![CDATA[ 
SELECT DISTINCT ep.`evd_podnio`, ep.`evd_odobrio`, ep.`evd_kontrolisao`,ep.`evd_locked`
, CONCAT(IFNULL(e1.`LastName`,''),' ',IFNULL(e1.`FirstName`,'')) AS empPodn
, CONCAT(IFNULL(e2.`LastName`,''),' ',IFNULL(e2.`FirstName`,'')) AS empOdob
, CONCAT(IFNULL(e3.`LastName`,''),' ',IFNULL(e3.`FirstName`,'')) AS empKont
FROM `evd_prisustva` ep 
LEFT JOIN `evd_employees` e1 ON ep.`evd_podnio`=e1.`EmployeeID`
LEFT JOIN `evd_employees` e2 ON ep.`evd_odobrio`=e2.`EmployeeID`
LEFT JOIN `evd_employees` e3 ON ep.`evd_kontrolisao`=e3.`EmployeeID`
WHERE ep.`employeeID`=@employeeID AND YEAR(ep.`datum`)=@Year AND MONTH(ep.`datum`)=@Month
AND (ep.`evd_podnio` IS NOT NULL  OR ep.`evd_odobrio` IS NOT NULL OR ep.`evd_kontrolisao` IS NOT NULL
OR ep.`evd_locked` <>0);
]]>.Value
            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employeeID", CurrentEmployeeId)
            mycmd.Parameters.AddWithValue("@Year", Year)
            mycmd.Parameters.AddWithValue("@Month", Month)
            mycmd.Prepare()

            Podnio = -1
            Odobrio = -1
            Kontrolisao = -1
            Locked = False

            OdobError = False

            EditEnabled = True
            NoEdit = False

            EmpPodnio = ""
            EmpOdobrio = ""
            EmpKontrolisao = ""

            dbRead = mycmd.ExecuteReader
            Do While dbRead.Read
                If dbRead.IsDBNull(0) Then
                    OdobError = True
                    Podnio = -1
                Else
                    Podnio = dbRead.GetInt32("evd_podnio")
                    EmpPodnio = dbRead.GetString("empPodn")
                End If

                If dbRead.IsDBNull(1) Then
                    OdobError = True
                    Odobrio = -1
                Else
                    Odobrio = dbRead.GetUInt32("evd_odobrio")
                    EmpOdobrio = dbRead.GetString("empOdob")
                End If

                If dbRead.IsDBNull(2) Then
                    OdobError = True
                    Kontrolisao = -1
                Else
                    Kontrolisao = dbRead.GetUInt32("evd_kontrolisao")
                    EmpKontrolisao = dbRead.GetString("empKont")
                End If

                If dbRead.IsDBNull(3) Then
                    OdobError = True
                    Locked = True
                Else
                    If dbRead.GetInt32("evd_locked") <> 0 Then Locked = True
                End If


            Loop
            dbRead.Close()

        End Using

        Select Case pEmpRole
            Case "uposlenik"
                If (Odobrio > 0 Or Kontrolisao > 0 Or Locked = True) Then
                    NoEdit = True
                    EditEnabled = False
                ElseIf (Podnio > 0) Then
                    EditEnabled = False
                End If

            Case "rukovodilac"
                If (Kontrolisao > 0 Or Locked = True) Then
                    NoEdit = True
                    EditEnabled = False
                ElseIf (Odobrio > 0) Then
                    EditEnabled = False
                End If

            Case "evidencija"
                If (Locked = True) Or (Odobrio < 0) Then
                    NoEdit = True
                    EditEnabled = False
                ElseIf (Kontrolisao > 0) Then
                    EditEnabled = False
                End If

        End Select




    End Sub

    Public Sub InsertDay(ByVal pDatum As Date, ByVal pVrstaPlacanja As String, ByVal pEmployeeID As Integer, Optional ByVal pRole As String = "uposlenik")
        '
        ' pDatum tretira kao MySql.Data.Types.MySqlDateTime
        '

        Dim mycmd As MySqlCommand
        Dim strSQL As String = <![CDATA[ 
    ]]>.Value

        Select Case pRole
            Case "uposlenik"
                strSQL = <![CDATA[ 
INSERT INTO evd_prisustva (employeeID, sifra_placanja, datum, vrijeme_od, vrijeme_do, vrijeme_ukupno)	VALUES(@employeeID, @selVrstaPlacanja, @datumStr, '08:00:00', '16:00:00' , '08:00:00');
    ]]>.Value

            Case "rukovodilac"
                strSQL = <![CDATA[ 
INSERT INTO evd_prisustva (employeeID, sifra_placanja, datum, vrijeme_od, vrijeme_do, vrijeme_ukupno, evd_podnio)	VALUES(@employeeID, @selVrstaPlacanja, @datumStr, '08:00:00', '16:00:00' , '08:00:00', @employeeIDPodn);
    ]]>.Value


            Case "evidencija"
                strSQL = <![CDATA[ 
INSERT INTO evd_prisustva (employeeID, sifra_placanja, datum, vrijeme_od, vrijeme_do, vrijeme_ukupno,  evd_podnio,  evd_odobrio)	VALUES(@employeeID, @selVrstaPlacanja, @datumStr, '08:00:00', '16:00:00' , '08:00:00', @employeeIDPodn, @employeeIDPodn);
    ]]>.Value

        End Select


        For Each dr In TblEvidDani.Rows

            Dim dtDatum As DateTime = dr("datum").ToString
            Dim dtVrstaPlacanja As String = dr("sifra_placanja")
            Dim prDatum As DateTime = pDatum.ToString

            If Not dr("datum") Is DBNull.Value Then

                If dtDatum.Equals(prDatum) Then
                    If dtVrstaPlacanja = pVrstaPlacanja Then
                        '
                        ' DELETE row
                        '
                        strSQL = <![CDATA[ 
DELETE FROM evd_prisustva  
WHERE employeeID=@employeeID AND sifra_placanja=@selVrstaPlacanja AND
datum=@datumStr;
    ]]>.Value
                    Else
                        Exit Sub
                    End If

                End If
            End If
        Next


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Try
                mycmd = New MySqlCommand
                mycmd.Connection = myconnection


                mycmd.CommandText = strSQL
    

                mycmd.Parameters.AddWithValue("@employeeID", Me.CurrentEmployeeId)
                mycmd.Parameters.AddWithValue("@selVrstaPlacanja", pVrstaPlacanja)
                mycmd.Parameters.AddWithValue("@datumStr", pDatum.ToString("yyyy.MM.dd"))
                mycmd.Prepare()

                If strSQL.Contains("@employeeIDPodn") Then
                    mycmd.Parameters.AddWithValue("@employeeIDPodn", pEmployeeID)
                End If

                mycmd.ExecuteNonQuery()


            Catch ex As Exception
                ex.ToString()
            End Try


        End Using



    End Sub

    Public Sub DeleteMonth(ByVal pVrstaPlacanja As String)

        Dim mycmd As MySqlCommand
        Dim strSQL As String = <![CDATA[ 
    ]]>.Value


        '
        ' DELETE all rows from month
        '
        strSQL = <![CDATA[ 
DELETE ep.* FROM `evd_prisustva` ep
WHERE ep.`employeeID`=@employeeID AND YEAR(ep.`datum`)=@Year AND MONTH(ep.`datum`)=@Month
AND ep.sifra_placanja=@pVrstaPlacanja;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Try
                mycmd = New MySqlCommand
                mycmd.Connection = myconnection


                mycmd.CommandText = strSQL
    

                mycmd.Parameters.AddWithValue("@employeeID", Me.CurrentEmployeeId)
                mycmd.Parameters.AddWithValue("@Year", Me.Year)
                mycmd.Parameters.AddWithValue("@Month", Me.Month)
                mycmd.Parameters.AddWithValue("@pVrstaPlacanja", pVrstaPlacanja)
                mycmd.Prepare()

                mycmd.ExecuteNonQuery()


            Catch ex As Exception
                ex.ToString()
            End Try


        End Using



    End Sub

    Public Sub InsertMonth(ByVal pVrstaPlacanja As String, ByVal pEmployeeID As Integer, Optional ByVal pRole As String = "uposlenik")

        Dim mycmd As MySqlCommand
        Dim strSQL As String = <![CDATA[ 
    ]]>.Value


        '
        ' DELETE all rows from month
        '


        Select Case pRole
            Case "uposlenik"
                strSQL = <![CDATA[ 
INSERT INTO `evd_prisustva` (`employeeID`, `sifra_placanja`, `datum`, vrijeme_od, vrijeme_do, `vrijeme_ukupno`)
SELECT @employeeID, @pVrstaPlacanja, sc.dt, '08:00:00', '16:00:00' , '08:00:00' FROM `sfr_calendar` sc 
LEFT JOIN (SELECT  ep.`datum` FROM `evd_prisustva` ep 
WHERE ep.`employeeID`=@employeeID AND YEAR(ep.`datum`)=@Year AND MONTH(ep.`datum`)=@Month) ep
ON sc.`dt`=ep.`datum`
WHERE sc.`isWeekday`=1 AND  sc.y=@Year AND sc.m=@Month
AND ep.`datum` IS NULL;
    ]]>.Value

            Case "rukovodilac"
                strSQL = <![CDATA[ 
INSERT INTO `evd_prisustva` (`employeeID`, `sifra_placanja`, `datum`, vrijeme_od, vrijeme_do,`vrijeme_ukupno`, evd_podnio)
SELECT @employeeID, @pVrstaPlacanja, sc.dt, '08:00:00', '16:00:00' , '08:00:00', @employeeIDPodn FROM `sfr_calendar` sc 
LEFT JOIN (SELECT  ep.`datum` FROM `evd_prisustva` ep 
WHERE ep.`employeeID`=@employeeID AND YEAR(ep.`datum`)=@Year AND MONTH(ep.`datum`)=@Month) ep
ON sc.`dt`=ep.`datum`
WHERE sc.`isWeekday`=1 AND  sc.y=@Year AND sc.m=@Month
AND ep.`datum` IS NULL;
    ]]>.Value


            Case "evidencija"
                strSQL = <![CDATA[ 
INSERT INTO evd_prisustva (employeeID, sifra_placanja, datum, vrijeme_od, vrijeme_do,vrijeme_ukupno)	VALUES(@employeeID, @selVrstaPlacanja, @datumStr, '08:00:00', '16:00:00' , '08:00:00');
    ]]>.Value

        End Select

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Try
                mycmd = New MySqlCommand
                mycmd.Connection = myconnection


                mycmd.CommandText = strSQL
    

                mycmd.Parameters.AddWithValue("@employeeID", Me.CurrentEmployeeId)
                mycmd.Parameters.AddWithValue("@Year", Me.Year)
                mycmd.Parameters.AddWithValue("@Month", Me.Month)
                mycmd.Parameters.AddWithValue("@pVrstaPlacanja", pVrstaPlacanja)
                mycmd.Prepare()

                If strSQL.Contains("@employeeIDPodn") Then
                    mycmd.Parameters.AddWithValue("@employeeIDPodn", pEmployeeID)
                End If

                mycmd.ExecuteNonQuery()


            Catch ex As Exception
                ex.ToString()
            End Try


        End Using



    End Sub

    Public Sub PodnesiEv(ByVal pEmployeePod As Integer, ByVal pRole As String, Optional ByVal pPonisti As Boolean = False)
        '
        ' pDatum tretira kao MySql.Data.Types.MySqlDateTime
        '

        Dim mycmd As MySqlCommand
        Dim strSQL As String = ""

        Dim strSQLPonisti As String = ""

        Select Case pRole
            Case "uposlenik"
                strSQL = <![CDATA[ 
UPDATE evd_prisustva SET evd_podnio = @employeeIDPod WHERE YEAR(datum) = @Year AND MONTH(datum) =@Month AND employeeID=@employeeID AND evd_locked=0
    ]]>.Value
                strSQLPonisti = <![CDATA[ 
UPDATE evd_prisustva SET evd_podnio = NULL WHERE YEAR(datum) = @Year And MONTH(datum) =@Month And employeeID=@employeeID And evd_locked=0
    ]]>.Value

            Case "rukovodilac"

                strSQL = <![CDATA[ 
UPDATE evd_prisustva SET evd_odobrio = @employeeIDPod WHERE YEAR(datum) = @Year AND MONTH(datum) =@Month AND employeeID=@employeeID AND evd_locked=0;
UPDATE evd_prisustva SET evd_odobrio = @employeeIDPod, evd_podnio = @employeeIDPod WHERE YEAR(datum) = @Year AND MONTH(datum) =@Month AND employeeID=@employeeID AND evd_locked=0
AND evd_podnio IS NULL;
    ]]>.Value
                strSQLPonisti = <![CDATA[ 
UPDATE evd_prisustva SET evd_odobrio = NULL WHERE YEAR(datum) = @Year And MONTH(datum) =@Month And employeeID=@employeeID And evd_locked=0;
    ]]>.Value



            Case "evidencija"
                strSQL = <![CDATA[ 
UPDATE evd_prisustva SET evd_kontrolisao = @employeeIDPod WHERE YEAR(datum) = @Year AND MONTH(datum) =@Month AND employeeID=@employeeID AND evd_locked=0
    ]]>.Value
                strSQLPonisti = <![CDATA[ 
UPDATE evd_prisustva SET evd_kontrolisao = NULL WHERE YEAR(datum) = @Year And MONTH(datum) =@Month And employeeID=@employeeID And evd_locked=0
    ]]>.Value

        End Select



        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Try
                mycmd = New MySqlCommand
                mycmd.Connection = myconnection


                If pPonisti Then
                    mycmd.CommandText = strSQLPonisti
                Else
                    mycmd.CommandText = strSQL
                End If

    

                If mycmd.CommandText.Contains("@employeeIDPod") Then mycmd.Parameters.AddWithValue("@employeeIDPod", pEmployeePod)
                mycmd.Parameters.AddWithValue("@employeeID", Me.CurrentEmployeeId)
                mycmd.Parameters.AddWithValue("@Year", Year)
                mycmd.Parameters.AddWithValue("@Month", Month)
                mycmd.Prepare()

                mycmd.ExecuteNonQuery()

            Catch ex As Exception
                ex.ToString()
            End Try


        End Using



    End Sub

End Class
