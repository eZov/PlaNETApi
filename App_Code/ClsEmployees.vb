Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsEmployees


    Public Sub New()

        DtList = New DataTable

        DtEmpDetails = New DataTable
        DtEmpDetailsLpd = New DataTable

        DtEmpDetailsGO = New DataTable
        DtEmpDetailsGOKrit = New DataTable

        DtEmpDetailsAGO = New DataTable
    End Sub

    Public LboxList As New List(Of ClsEmployee)
    Public Property DtList As DataTable

    Public Property DtEmpDetails As DataTable
    Public Property DrEmpDetails As DataRow

    Public Property DtEmpDetailsLpd As DataTable
    Public Property DrEmpDetailsLpd As DataRow

    Public Property DtEmpDetailsGO As DataTable
    Public Property DrEmpDetailsGO As DataRow

    Public Property DtEmpDetailsGOKrit As DataTable

    Public Property DtEmpDetailsAGO As DataTable
    Public Property DrEmpDetailsAGO As DataRow

    Public Function getList(ByVal pDbTableName As String, ByVal pSfr As String) As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand

        If pDbTableName Is Nothing Or pSfr Is Nothing Then
            Return DtList
        End If

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT so.`Sifra`,  e.`EmployeeID` AS id, CONCAT(e.`LastName`,' ',e.`FirstName`,' (',e.EmployeeID,')') AS Naziv    FROM `evd_employees` e LEFT JOIN `sfr_organizacija` so ON e.`DepartmentUP`=so.`id`
WHERE so.`Sifra` REGEXP '^@Sifra' AND e.Aktivan <> 0 ORDER BY e.`LastName` LIMIT 0,3;
]]>.Value

            strSQL = <![CDATA[ 
        Call EMPwb_get_list('?DbTableName', '@Sifra');
]]>.Value

            strSQL = strSQL.Replace("?DbTableName", pDbTableName)
            mycmd.CommandText = strSQL.Replace("@Sifra", pSfr)



            'mycmd.Parameters.AddWithValue("@row_number", 0)

            DtList.Rows.Clear()
            myda = New MySqlClient.MySqlDataAdapter(mycmd)
            myda.Fill(DtList)

        End Using


        LboxList.Clear()

        For Each er As DataRow In DtList.Rows
            LboxList.Add(New ClsEmployee With {.EmployeeId = er.Item("id"), .LastName = er.Item("Naziv")})
        Next

        Return DtList

    End Function

    Public Function getIdx(Optional ByVal pEmpId As Integer = -1) As Integer

        If pEmpId = -1 Then Return -1
        Dim idx As Integer = -1

        For Each er In DtList.Rows
            idx += 1
            If er("Id") = pEmpId Then
                Exit For
            End If
        Next

        Return idx
    End Function

    Public Function getEmpDetails(ByVal pEmpId As String) As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT
  EmployeeID, EmployeeNumber, JMB, FirstName, MiddleName, LastName, Title, BrSocOsig, Opc_Stanovanja, Opc_Porodice, Opc_PIO, Opc_ZDR, Opc_ZPS, Djeca, RadnoMjesto, Kat_Ovlastenja, PlatniRazred, Koef_D1, Koef_D2, Koef_D3, Koef_D4, Koef_D5, Koef_D6, Koef_D7, Plata_Ukupno, Proc_MinRad, Prosj_1m, Prosj_3m, Prosj_6m, Satnica_Bol, Prevoz, Isplata, Banka_1, Banka_2, TR_racun_1, TR_racun_2, TR_Default, PostalCode, DepartmentID, OfficeLocation, 
  IF(Dat_Rodjenja='0000-00-00','1900-01-01',Dat_Rodjenja) AS Dat_Rodjenja, 
  IF(Dat_Zaposl='0000-00-00','1900-01-01',Dat_Zaposl) AS Dat_Zaposl, 
  IF(Dat_Odlaska='0000-00-00','1900-01-01',Dat_Odlaska) AS Dat_Odlaska, 
Aktivan, Staz_Ukupno, Staz_Kontinuitet, Staz_Prethodni, Razlog_Odlaska, DoprinosiSumaProc, Reserve_1, Reserve_2, DummyTime, Staz_UkupnoGodina, DepartmentUP, Opc_PSL, MenUgovor, MenUgovor_Iznos, 
  IF(MenUgovor_DatPrest='0000-00-00','1900-01-01',MenUgovor_DatPrest) AS MenUgovor_DatPrest, 
Pol, USER, IznosProsjekPlate, Bol_TrudIznos, Bol_TrudRjes, obracun, PorKart_16, PorKart_15, PorKart_14, PorKart_13, PorKart_12
FROM evd_employees WHERE EmployeeID=@EmpId;
]]>.Value
            mycmd.CommandText = strSQL

            mycmd.Parameters.AddWithValue("@EmpId", pEmpId)
            mycmd.Prepare()

            myda = New MySqlClient.MySqlDataAdapter(mycmd)
            DtEmpDetails.Rows.Clear()

            myda.Fill(DtEmpDetails)

            strSQL = <![CDATA[ 
SELECT
  EmployeeID, JMB, DummyTime, ImeRoditelja, Broj_LK, Mjesto_LK, Opcina_LK, Pol, 
  IF(Dat_Rodjenja='0000-00-00','1900-01-01',Dat_Rodjenja) AS Dat_Rodjenja, 
Mjesto_Rodjenja, 
  Opcina_Rodjenja, Drzava_Rodjenja, Drzavljanstvo, Narod, BracnoStanje, Supruznik, Djeca, KrvnaGrupa, KontaktOsoba, 
  Pripravnik, StatusZaposlenja, DjecijiDodatak, TerenskiDodatak, BrojClanovaDomacinstva, StrucnaSprema, NazivZavrseneSkole, 
  NazivTekuceSkole, Zanimanje, StrucniIspit, SpecijalistickaObuka, PosebnaZnanja, PoznavanjeRacunara, OcjenaRada, 
  PovredaRadneDiscipline, Invalidnost, SamohraniRoditelj, Adresa, Tel_pos, Tel_prv, Mob_prv, Email_pos, adresa_br
FROM
  evd_employees_lpd WHERE EmployeeID=@EmpId;
]]>.Value
            mycmd.CommandText = strSQL


            'mycmd.Parameters.AddWithValue("@EmpId", pEmpId)

            myda = New MySqlClient.MySqlDataAdapter(mycmd)
            DtEmpDetailsLpd.Rows.Clear()

            myda.Fill(DtEmpDetailsLpd)

        End Using

        If DtEmpDetails.Rows.Count > 0 Then
            DrEmpDetails = DtEmpDetails.Rows(0)
            DrEmpDetailsLpd = DtEmpDetailsLpd.Rows(0)
        End If


        Return DtEmpDetails

    End Function

    Public Function getEmpDetailsGO(ByVal pEmpId As String, ByVal pYear As Integer) As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Dim loopRetry As Integer = 1

CreateNewGO:

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT
    ego.employeeID
    , ego.Godina
    , ego.IzrDani
    , ego.IskDani
    , ego.IskDani2
    , ego.Staz_G
    , ego.Staz_M
    , ego.Staz_D
    , egop1.startDate AS startDate1
    , egop1.endDate AS endDate1
    , egop1.radniDate AS radniDate1
    , egop1.brojDana AS brojDana1
    , egop2.startDate AS startDate2
    , egop2.endDate AS endDate2
    , egop2.radniDate AS radniDate2
    , egop2.brojDana AS brojDana2
    , ego.NeIskDani
    , ego.NeIskDaniPreneseni
    , ego.IzrDaniPreneseni
    , ego.NeIskDaniAnul
    , ego.IzrDaniAnul
    , ego.rjesenje_status
    , ego.evd_locked
FROM
    evd_godisnjiodmor AS ego
    INNER JOIN evd_godisnjiodmor_plan AS egop1 
        ON (ego.employeeID = egop1.employeeID) AND (ego.Godina = egop1.godina)
    INNER JOIN evd_godisnjiodmor_plan AS egop2
        ON (egop1.employeeID = egop2.employeeID) AND (egop1.godina = egop2.godina)
WHERE (ego.employeeID =@EmpId
    AND ego.Godina =@Year
    AND egop1.period =1
    AND egop2.period =2);
]]>.Value
            mycmd.CommandText = strSQL

            mycmd.Parameters.AddWithValue("@EmpId", pEmpId)
            mycmd.Parameters.AddWithValue("@Year", pYear)
            mycmd.Prepare()

            myda = New MySqlClient.MySqlDataAdapter(mycmd)
            DtEmpDetailsGO.Clear()
            myda.Fill(DtEmpDetailsGO)

        End Using

        If DtEmpDetailsGO.Rows.Count > 0 Then
            DrEmpDetailsGO = DtEmpDetailsGO.Rows(0)
        ElseIf loopRetry < 2 Then
            createEmpDetailGo(pEmpId, pYear)
            loopRetry += 1
            GoTo CreateNewGO
        End If


        Return DtEmpDetailsGO

    End Function

    Public Function getEmpDetailsGOKrit(ByVal pEmpId As String, ByVal pYear As Integer) As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT DISTINCT
    `egop`.`Naziv`
    , `egop`.`Forma`
    , `egod`.`brojDana`
    , `egod`.`evd_go_id`
    , egop.Kriterij
FROM
    `evd_godisnjiodmor_postavke` AS `egop`
    INNER JOIN `evd_godisnjiodmor_dod` AS `egod` 
        ON (`egop`.`Kriterij` = `egod`.`kriterij`)
WHERE (`egod`.`employeeID` =@EmpId
    AND `egod`.`godina` =@Year)
ORDER BY `egop`.`Naziv` ASC;
]]>.Value
            mycmd.CommandText = strSQL

            mycmd.Parameters.AddWithValue("@EmpId", pEmpId)
            mycmd.Parameters.AddWithValue("@Year", pYear)
            mycmd.Prepare()

            myda = New MySqlClient.MySqlDataAdapter(mycmd)
            DtEmpDetailsGOKrit.Clear()
            myda.Fill(DtEmpDetailsGOKrit)

        End Using

        Return DtEmpDetailsGO

    End Function

    Public Function getEmpDetailsAGO(ByVal pEmpId As String, ByVal pYear As Integer) As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT * FROM a_evd_godisnjiodmor ago LEFT JOIN evd_doc_documents ed ON ago.doc_md5=ed.md5
WHERE ago.employeeID=@EmpId AND ago.Godina=@Year 
AND ago.rjesenje_status=-1
ORDER BY ed.DummyTime LIMIT 0,1;
]]>.Value
            mycmd.CommandText = strSQL

            mycmd.Parameters.AddWithValue("@EmpId", pEmpId)
            mycmd.Parameters.AddWithValue("@Year", pYear)
            mycmd.Prepare()

            myda = New MySqlClient.MySqlDataAdapter(mycmd)
            DtEmpDetailsAGO.Clear()
            myda.Fill(DtEmpDetailsAGO)

        End Using

        If DtEmpDetailsAGO.Rows.Count > 0 Then
            DrEmpDetailsAGO = DtEmpDetailsAGO.Rows(0)
        End If

        Return DtEmpDetailsAGO

    End Function

    Public Sub createEmpDetailGo(ByVal pEmpId As String, ByVal pYear As Integer)

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
CALL GO_PripremaTabelaEmp( @EmployeeID, @Godina, @BaseDb);
]]>.Value
            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeID", pEmpId)
            mycmd.Parameters.AddWithValue("@Godina", pYear)
            mycmd.Parameters.AddWithValue("@BaseDb", "zovco001_gonet_test")
            mycmd.Prepare()

            mycmd.ExecuteNonQuery()


        End Using


    End Sub

    Public Sub insertEmpDetail(ByRef pEmpDetail As ClsEmployee)

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
INSERT INTO evd_employees (EmployeeNumber,  FirstName, MiddleName, LastName, BrSocOsig,  
                           Opc_Stanovanja, Opc_Porodice, Opc_PIO, Opc_ZDR, Opc_ZPS, 
                           Koef_D7,  DepartmentID, DepartmentUP)
VALUES
  ( @EmployeeNumber, @FirstName, @MiddleName, 
  @LastName, @BrSocOsig, @Opc_Stanovanja, @Opc_Stanovanja, 
  @Opc_Stanovanja, @Opc_Stanovanja, @Opc_Stanovanja,  
  @Koef_D7, @DepartmentID, @DepartmentUP );
]]>.Value
            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            'mycmd.Parameters.AddWithValue("@EmployeeID", pEmpDetail.EmployeeId)

            mycmd.Parameters.AddWithValue("@EmployeeNumber", pEmpDetail.PersBroj)
            mycmd.Parameters.AddWithValue("@FirstName", pEmpDetail.FirstName)
            mycmd.Parameters.AddWithValue("@MiddleName", pEmpDetail.BornName)
            mycmd.Parameters.AddWithValue("@LastName", pEmpDetail.LastName)
            mycmd.Parameters.AddWithValue("@BrSocOsig", pEmpDetail.SocOsBroj)
            mycmd.Parameters.AddWithValue("@Opc_Stanovanja", pEmpDetail.OpcStanovanja)
            mycmd.Parameters.AddWithValue("@Koef_D7", pEmpDetail.NepunoRadVrProc)
            mycmd.Parameters.AddWithValue("@DepartmentID", pEmpDetail.OrgJedObr)
            mycmd.Parameters.AddWithValue("@DepartmentUP", pEmpDetail.OrgJedUpo)
            mycmd.Prepare()

            mycmd.ExecuteNonQuery()
            pEmpDetail.EmployeeId = mycmd.LastInsertedId


            strSQL = <![CDATA[ 
INSERT INTO evd_employees_lpd (EmployeeID)
VALUES ( @EmployeeID );
]]>.Value
            mycmd.CommandText = strSQL

            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeID", pEmpDetail.EmployeeId)
            mycmd.Prepare()

            mycmd.ExecuteNonQuery()

            strSQL = <![CDATA[ 
INSERT INTO evd_employees_ext (EmployeeID)
VALUES ( @EmployeeID );
]]>.Value
            mycmd.CommandText = strSQL

            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeID", pEmpDetail.EmployeeId)
            mycmd.Prepare()

            mycmd.ExecuteNonQuery()
        End Using


    End Sub

    Public Sub updateEmpDetail(ByRef pEmpDetail As ClsEmployee)

        Dim mycmd As MySqlCommand
        pEmpDetail.Pol = "N"

        If pEmpDetail.PolF Then
            pEmpDetail.Pol = "F"
        ElseIf pEmpDetail.PolM Then
            pEmpDetail.Pol = "M"
        End If

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
UPDATE evd_employees
SET
  EmployeeNumber = @EmployeeNumber, FirstName = @FirstName, MiddleName = @MiddleName, 
  LastName = @LastName, JMB = @JMB, BrSocOsig = @BrSocOsig, 
  Opc_Stanovanja = @Opc_Stanovanja, Opc_Porodice = @Opc_Stanovanja, 
  Opc_PIO = @Opc_Stanovanja, Opc_ZDR = @Opc_Stanovanja, Opc_ZPS = @Opc_Stanovanja,  
  Koef_D7 = @Koef_D7, 
  DepartmentID = @DepartmentID, DepartmentUP = @DepartmentUP,
  Pol = @Pol
WHERE EmployeeID = @EmployeeID;
]]>.Value
            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeID", pEmpDetail.EmployeeId)

            mycmd.Parameters.AddWithValue("@EmployeeNumber", pEmpDetail.PersBroj)
            mycmd.Parameters.AddWithValue("@FirstName", pEmpDetail.FirstName)
            mycmd.Parameters.AddWithValue("@MiddleName", pEmpDetail.BornName)
            mycmd.Parameters.AddWithValue("@LastName", pEmpDetail.LastName)

            mycmd.Parameters.AddWithValue("@JMB", pEmpDetail.JMB)

            mycmd.Parameters.AddWithValue("@BrSocOsig", pEmpDetail.SocOsBroj)
            mycmd.Parameters.AddWithValue("@Opc_Stanovanja", pEmpDetail.OpcStanovanja)
            mycmd.Parameters.AddWithValue("@Koef_D7", pEmpDetail.NepunoRadVrProc)
            mycmd.Parameters.AddWithValue("@DepartmentID", pEmpDetail.OrgJedObr)
            mycmd.Parameters.AddWithValue("@DepartmentUP", pEmpDetail.OrgJedUpo)
            mycmd.Parameters.AddWithValue("@Pol", pEmpDetail.Pol)
            mycmd.Prepare()

            mycmd.ExecuteNonQuery()


        End Using


    End Sub

    Public Sub updateEmpDetailRadMje(ByRef pEmpDetail As ClsEmployee)

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
UPDATE evd_employees
SET
  RadnoMjesto = @RadnoMjesto
WHERE EmployeeID = @EmployeeID;
]]>.Value
            mycmd.CommandText = strSQL

            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeID", pEmpDetail.EmployeeId)
            mycmd.Parameters.AddWithValue("@RadnoMjesto", pEmpDetail.RadMjes)
            mycmd.Prepare()

            mycmd.ExecuteNonQuery()

            strSQL = <![CDATA[ 
UPDATE evd_employees_lpd
SET
  StrucnaSprema = @StrucnaSprema
WHERE EmployeeID = @EmployeeID;
]]>.Value
            mycmd.CommandText = strSQL

            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeID", pEmpDetail.EmployeeId)
            mycmd.Parameters.AddWithValue("@StrucnaSprema", pEmpDetail.StrSpr)
            mycmd.Prepare()

            mycmd.ExecuteNonQuery()

        End Using


    End Sub

    Public Sub updateEmpDetailStaz(ByRef pEmpDetail As ClsEmployee)

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
UPDATE evd_employees
SET
  Staz_Prethodni  = @Staz_Prethodni,
  Dat_Zaposl     = @Dat_Zaposl,
  Dat_Odlaska    = @Dat_Odlaska
WHERE EmployeeID = @EmployeeID;
]]>.Value
            mycmd.CommandText = strSQL

            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeID", pEmpDetail.EmployeeId)

            mycmd.Parameters.AddWithValue("@Staz_Prethodni", pEmpDetail.stazPret)
            mycmd.Parameters.AddWithValue("@Dat_Zaposl", pEmpDetail.DatZaposlenja.ToString("yyyy-MM-dd"))
            mycmd.Parameters.AddWithValue("@Dat_Odlaska", pEmpDetail.DateOdlaska.ToString("yyyy-MM-dd"))

            mycmd.Prepare()

            mycmd.ExecuteNonQuery()

        End Using


    End Sub

    Public Sub updateEmpDetailProracStaz(ByRef pEmpDetail As ClsEmployee, ByVal pDatDo As Date)

        Dim mycmd As MySqlCommand
        Dim _SUk As Double = 0
        Dim _SKo As Double = 0

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String
            strSQL = <![CDATA[ 
Emp_StazEmp
]]>.Value
            mycmd.CommandText = strSQL
            mycmd.CommandType = CommandType.StoredProcedure

            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@pEmployee", pEmpDetail.EmployeeId)
            mycmd.Parameters("@pEmployee").Direction = ParameterDirection.Input

            mycmd.Parameters.AddWithValue("@pDate", pDatDo.ToString("yyyy-MM-dd"))
            mycmd.Parameters("@pDate").Direction = ParameterDirection.Input

            mycmd.Prepare()

            'mycmd.Parameters.AddWithValue("@pSUk", MySqlDbType.Double)
            'mycmd.Parameters("@pSUk").Direction = ParameterDirection.Output

            'mycmd.Parameters.AddWithValue("@pSKo", MySqlDbType.Double)
            'mycmd.Parameters("@pSKo").Direction = ParameterDirection.Output

            Try
                mycmd.ExecuteNonQuery()
            Catch ex As MySqlException

            End Try

        End Using


    End Sub

    Public Sub updateEmpDetailLpd(ByRef pEmpDetail As ClsEmployee)

        Dim mycmd As MySqlCommand
        pEmpDetail.Pol = "N"

        If pEmpDetail.PolF Then
            pEmpDetail.Pol = "F"
        ElseIf pEmpDetail.PolM Then
            pEmpDetail.Pol = "M"
        End If

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
UPDATE evd_employees_lpd
SET 
  Adresa = @Adresa, adresa_br = @adresa_br, Pol = @Pol
WHERE EmployeeID = @EmployeeID;
]]>.Value
            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeID", pEmpDetail.EmployeeId)

            mycmd.Parameters.AddWithValue("@Adresa", pEmpDetail.Adresa)
            mycmd.Parameters.AddWithValue("@adresa_br", pEmpDetail.AdresaBr)
            mycmd.Parameters.AddWithValue("@Pol", pEmpDetail.Pol)

            mycmd.Prepare()

            mycmd.ExecuteNonQuery()


        End Using


    End Sub

    Public Sub updateEmpDetailGo1(ByRef pEmpDetail As ClsEmployeeGodOdm)

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
UPDATE evd_godisnjiodmor_plan
SET
  startDate = @startDate, endDate = @endDate, radniDate = @radniDate, brojDana = @brojDana 
WHERE godina = @godina
  AND rjesenje_status = 0
  AND period = 1
  AND EmployeeID = @EmployeeID;
]]>.Value
            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@startDate", pEmpDetail.plan1StartDate)
            mycmd.Parameters.AddWithValue("@endDate", pEmpDetail.plan1EndDate)
            mycmd.Parameters.AddWithValue("@radniDate", pEmpDetail.plan1RadDate)
            mycmd.Parameters.AddWithValue("@brojDana", pEmpDetail.plan1Dani)

            mycmd.Parameters.AddWithValue("@EmployeeID", pEmpDetail.EmployeeId)
            mycmd.Parameters.AddWithValue("@godina", pEmpDetail.Godina)

            mycmd.Prepare()

            mycmd.ExecuteNonQuery()

        End Using


    End Sub

    Public Sub updateEmpDetailGo2(ByRef pEmpDetail As ClsEmployeeGodOdm)

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String

            strSQL = <![CDATA[ 
UPDATE evd_godisnjiodmor_plan
SET
  startDate = @startDate, endDate = @endDate, radniDate = @radniDate, brojDana = @brojDana 
WHERE godina = @godina
  AND rjesenje_status = 0
  AND period = 2
  AND EmployeeID = @EmployeeID;
]]>.Value
            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@startDate", pEmpDetail.plan2StartDate)
            mycmd.Parameters.AddWithValue("@endDate", pEmpDetail.plan2EndDate)
            mycmd.Parameters.AddWithValue("@radniDate", pEmpDetail.plan2RadDate)
            mycmd.Parameters.AddWithValue("@brojDana", pEmpDetail.plan2Dani)

            mycmd.Parameters.AddWithValue("@EmployeeID", pEmpDetail.EmployeeId)
            mycmd.Parameters.AddWithValue("@godina", pEmpDetail.Godina)

            mycmd.Prepare()

            mycmd.ExecuteNonQuery()
        End Using


    End Sub

    Public Sub updateEmpDetailGOKrit(ByRef pRows As ArrayList)

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
UPDATE evd_godisnjiodmor_dod
SET brojDana = @brojDana
WHERE evd_go_id = @evd_go_id;
]]>.Value
            mycmd.CommandText = strSQL

            For Each el As Dictionary(Of String, Object) In pRows
                Dim rowKey As Integer = CInt(el("evd_go_id"))
                Dim rowVal As Integer = CInt(el("brojDana"))
                Dim rowForm As Integer = CInt(el("Forma"))

                If rowForm > 0 Then

                    mycmd.Parameters.Clear()
                    mycmd.Parameters.AddWithValue("@evd_go_id", rowKey)
                    mycmd.Parameters.AddWithValue("@brojDana", rowVal)
                    mycmd.Prepare()

                    mycmd.ExecuteNonQuery()
                End If

            Next

        End Using


    End Sub

    Public Sub updateEmpDetailGoAnPr(ByRef pEmpDetail As ClsEmployeeGodOdm)

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
UPDATE
  evd_godisnjiodmor
SET
  NeIskDani = @NeIskDani, 
  NeIskDaniAnul = @NeIskDaniAnul, 
  NeIskDaniPreneseni = @NeIskDaniPreneseni, 
  IzrDaniPreneseni = @IzrDaniPreneseni, 
  IzrDaniAnul = @IzrDaniAnul
WHERE EmployeeID = @EmployeeID
  AND Godina = @Godina;
]]>.Value
            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@NeIskDaniPreneseni", pEmpDetail.PreneseniSGO)
            mycmd.Parameters.AddWithValue("@IzrDaniPreneseni", pEmpDetail.PreneseniNGO)
            mycmd.Parameters.AddWithValue("@NeIskDaniAnul", pEmpDetail.AnulSGO)
            mycmd.Parameters.AddWithValue("@IzrDaniAnul", pEmpDetail.AnulNGO)
            mycmd.Parameters.AddWithValue("@NeIskDani", pEmpDetail.DaniSGO)

            mycmd.Parameters.AddWithValue("@EmployeeID", pEmpDetail.EmployeeId)
            mycmd.Parameters.AddWithValue("@Godina", pEmpDetail.Godina)

            mycmd.Prepare()

            mycmd.ExecuteNonQuery()


        End Using


    End Sub

    Public Sub updateEmpDetailObr(ByRef pEmpDetail As ClsEmployee)

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
UPDATE evd_employees
SET
    Banka_1 = @Banka_1, 
    TR_racun_1 = @TR_racun_1, 
    PlatniRazred = @PlatniRazred, 
    Plata_Ukupno = @Plata_Ukupno
WHERE EmployeeID = @EmployeeID;
]]>.Value
            mycmd.CommandText = strSQL

            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeID", pEmpDetail.EmployeeId)

            mycmd.Parameters.AddWithValue("@Banka_1", pEmpDetail.Banka)
            mycmd.Parameters.AddWithValue("@TR_racun_1", pEmpDetail.TekRacun)
            mycmd.Parameters.AddWithValue("@PlatniRazred", pEmpDetail.BrutoPlata)
            mycmd.Parameters.AddWithValue("@Plata_Ukupno", pEmpDetail.NetoPlata)

            mycmd.Prepare()

            mycmd.ExecuteNonQuery()


        End Using


    End Sub

    Public Sub updateEmpDetailObrPorez(ByRef pEmpDetail As ClsEmployee)

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
UPDATE evd_employees
SET
    PorKart_12 = @PorKart_12, 
    PorKart_13 = @PorKart_13, 
    PorKart_14 = @PorKart_14, 
    PorKart_15 = @PorKart_15,
    PorKart_16 = @PorKart_16
WHERE EmployeeID = @EmployeeID;
]]>.Value
            mycmd.CommandText = strSQL

            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeID", pEmpDetail.EmployeeId)

            mycmd.Parameters.AddWithValue("@PorKart_12", pEmpDetail.PorKoef12)
            mycmd.Parameters.AddWithValue("@PorKart_13", pEmpDetail.PorKoef13)
            mycmd.Parameters.AddWithValue("@PorKart_14", pEmpDetail.PorKoef14)
            mycmd.Parameters.AddWithValue("@PorKart_15", pEmpDetail.PorKoef15)
            mycmd.Parameters.AddWithValue("@PorKart_16", pEmpDetail.PorKoef16)

            mycmd.Prepare()

            mycmd.ExecuteNonQuery()


        End Using


    End Sub

    Public Sub updateEmpDetailObrBeneficije(ByRef pEmpDetail As ClsEmployee)

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
UPDATE evd_employees
SET
    Bol_TrudIznos = @Bol_TrudIznos, 
    Bol_TrudRjes = @Bol_TrudRjes,
    Reserve_1 = @Reserve_1,
    Reserve_2 = @Reserve_2,
    MenUgovor = @MenUgovor
WHERE EmployeeID = @EmployeeID;
]]>.Value
            mycmd.CommandText = strSQL

            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeID", pEmpDetail.EmployeeId)

            mycmd.Parameters.AddWithValue("@Bol_TrudIznos", pEmpDetail.PorodIznos)
            mycmd.Parameters.AddWithValue("@Bol_TrudRjes", pEmpDetail.PorodRjes)

            mycmd.Parameters.AddWithValue("@Reserve_1", pEmpDetail.Reserve_1)
            mycmd.Parameters.AddWithValue("@Reserve_2", pEmpDetail.Reserve_2)

            mycmd.Prepare()

            Dim _menUgovor As String = ""

            Try
                _menUgovor = pEmpDetail.PorodOd.ToString("yyMMdd") + "-" + pEmpDetail.PorodDo.ToString("yyMMdd")
            Catch ex As Exception

            End Try

            mycmd.Parameters.AddWithValue("@MenUgovor", _menUgovor)

            mycmd.ExecuteNonQuery()


        End Using


    End Sub

    Public Function EmpDetails(ByVal pColName As String) As Object
        Return DrEmpDetails.Item(pColName)
    End Function

    Public Function EmpDetails() As ClsEmployee

        Dim cEmployee As New ClsEmployee

        cEmployee.EmployeeId = CInt(DrEmpDetails.Item("employeeID"))

        cEmployee.FirstName = CStr(DrEmpDetails.Item("FirstName"))
        cEmployee.LastName = CStr(DrEmpDetails.Item("LastName"))

        cEmployee.JMB = CStr(DrEmpDetails.Item("JMB"))
        cEmployee.PersBroj = CStr(DrEmpDetails.Item("EmployeeNumber"))
        cEmployee.SocOsBroj = CStr(DrEmpDetails.Item("BrSocOsig"))

        cEmployee.BornName = CStr(DrEmpDetails.Item("MiddleName"))

        cEmployee.Pol = CStr(DrEmpDetails.Item("Pol"))
        cEmployee.PolF = False
        cEmployee.PolM = False

        Select Case cEmployee.Pol
            Case "F"
                cEmployee.PolF = True
            Case "M"
                cEmployee.PolM = True
        End Select


        cEmployee.OrgJedObr = CInt(DrEmpDetails.Item("DepartmentID"))
        cEmployee.OrgJedUpo = CInt(DrEmpDetails.Item("DepartmentUP"))

        cEmployee.RadMjes = CInt(DrEmpDetails.Item("RadnoMjesto"))
        cEmployee.StrSpr = CStr(DrEmpDetailsLpd.Item("StrucnaSprema"))

        cEmployee.OpcStanovanja = CStr(DrEmpDetails.Item("Opc_Stanovanja"))

        cEmployee.Adresa = CStr(DrEmpDetailsLpd.Item("Adresa"))
        cEmployee.AdresaBr = CStr(DrEmpDetailsLpd.Item("adresa_br"))

        cEmployee.NepunoRadVrProc = CInt(DrEmpDetails.Item("Koef_D7"))
        If cEmployee.NepunoRadVrProc > 0 Then
            cEmployee.NepunoRadVr = True
        Else
            cEmployee.NepunoRadVr = False
        End If

        cEmployee.DatZaposlenja = DateTime.Parse(DrEmpDetails("Dat_Zaposl").ToString())
        cEmployee.DateOdlaska = DateTime.Parse(DrEmpDetails("Dat_Odlaska").ToString())

        cEmployee.stazPret = CDbl(DrEmpDetails.Item("Staz_Prethodni"))
        'cEmployee.stazPretPoslodavac = CDbl(DrEmpDetails.Item("Staz_Prethodni"))
        cEmployee.stazKont = CDbl(DrEmpDetails.Item("Staz_Kontinuitet"))
        cEmployee.stazUkup = CDbl(DrEmpDetails.Item("Staz_Ukupno"))

        cEmployee.procMinRad = CDbl(DrEmpDetails.Item("Proc_MinRad"))

        cEmployee.Banka = CStr(DrEmpDetails.Item("Banka_1"))
        cEmployee.TekRacun = CStr(DrEmpDetails.Item("TR_racun_1"))

        cEmployee.BrutoPlata = CDbl(DrEmpDetails.Item("PlatniRazred"))
        cEmployee.NetoPlata = CDbl(DrEmpDetails.Item("Plata_Ukupno"))

        cEmployee.PorKoef12 = CDbl(DrEmpDetails.Item("PorKart_12"))
        cEmployee.PorKoef13 = CDbl(DrEmpDetails.Item("PorKart_13"))
        cEmployee.PorKoef14 = CDbl(DrEmpDetails.Item("PorKart_14"))
        cEmployee.PorKoef15 = CDbl(DrEmpDetails.Item("PorKart_15"))
        cEmployee.PorKoef16 = CDbl(DrEmpDetails.Item("PorKart_16"))

        cEmployee.PorKoef17 = cEmployee.PorKoef12 + cEmployee.PorKoef13 + cEmployee.PorKoef14 + cEmployee.PorKoef15 + cEmployee.PorKoef16

        cEmployee.PorodIznos = CDbl(DrEmpDetails.Item("Bol_TrudIznos"))
        cEmployee.PorodRjes = CStr(DrEmpDetails.Item("Bol_TrudRjes"))

        Dim _menUgovorDb As String = CStr(DrEmpDetails.Item("MenUgovor"))

        If _menUgovorDb.Length >= 13 Then

            Dim _menUgovor As String() = _menUgovorDb.Split("-")

            Dim _YOd, _MOd, _DOd As Integer
            Dim _YDo, _MDo, _DDo As Integer

            Integer.TryParse(_menUgovor(0).Substring(0, 2), _YOd)
            Integer.TryParse(_menUgovor(0).Substring(2, 2), _MOd)
            Integer.TryParse(_menUgovor(0).Substring(4, 2), _DOd)

            Integer.TryParse(_menUgovor(1).Substring(0, 2), _YDo)
            Integer.TryParse(_menUgovor(1).Substring(2, 2), _MDo)
            Integer.TryParse(_menUgovor(1).Substring(4, 2), _DDo)

            If _menUgovor(0).Substring(0, 2) = "00" Then
                cEmployee.PorodOd = New Date(1900, 1, 1)
                cEmployee.PorodDo = New Date(1900, 1, 1)
            Else
                cEmployee.PorodOd = New Date(_YOd, _MOd, _DOd)
                cEmployee.PorodDo = New Date(_YDo, _MDo, _DDo)
            End If


        Else
            cEmployee.PorodOd = New Date(1900, 1, 1)
            cEmployee.PorodDo = New Date(1900, 1, 1)

        End If



        cEmployee.Reserve_1 = CStr(DrEmpDetails.Item("Reserve_1"))
        cEmployee.Reserve_2 = CStr(DrEmpDetails.Item("Reserve_2"))

        EmpDetailsXrf(cEmployee)

        Return cEmployee

    End Function

    Public Sub EmpDetailsXrf(ByRef cEmployee As ClsEmployee)

        Dim cOrganizacija = New ClsSfrOrganizacija
        Dim cSistematizacija = New ClsSfrSistematizacija

        cOrganizacija.getList(cEmployee.OrgJedUpo)
        cEmployee.OrgJedUpoXfr = cOrganizacija.rowDetails(cEmployee.OrgJedUpo, "oNaziv")

        cOrganizacija.getList(cEmployee.OrgJedObr)
        cEmployee.OrgJedObrXfr = cOrganizacija.rowDetails(cEmployee.OrgJedObr, "oNaziv")

        cSistematizacija.getList(cEmployee.RadMjes)
        cEmployee.RadMjesXfr = cSistematizacija.rowDetails(cEmployee.RadMjes, "Naziv")
    End Sub

    ''' <summary>
    ''' Vraća kao ClsEmployeeGodOdm iz tabele DrEmpDetailsGO
    ''' </summary>
    ''' <returns></returns>
    Public Function EmpDetailsGO() As ClsEmployeeGodOdm

        Dim cEmployeeGodOdm As New ClsEmployeeGodOdm

        If DtEmpDetailsGOKrit.Rows.Count > 0 Then
            cEmployeeGodOdm.Kriteriji = DtEmpDetailsGOKrit
        Else
            cEmployeeGodOdm.Kriteriji = Nothing
        End If


        If DrEmpDetailsGO IsNot Nothing AndAlso DrEmpDetailsGO.Table.Rows.Count > 0 Then
            cEmployeeGodOdm.EmployeeId = CInt(DrEmpDetailsGO.Item("employeeID"))
            cEmployeeGodOdm.Godina = CInt(DrEmpDetailsGO.Item("Godina"))



            cEmployeeGodOdm.DaniGO = CInt(DrEmpDetailsGO("IzrDani"))

            cEmployeeGodOdm.Staz = String.Format("{0} god {1} mje  {2} dan", DrEmpDetailsGO("Staz_G").ToString(), DrEmpDetailsGO("Staz_M").ToString(),
                                                  DrEmpDetailsGO("Staz_D").ToString())

            cEmployeeGodOdm.RjesStatus = CInt(DrEmpDetailsGO.Item("rjesenje_status"))
            cEmployeeGodOdm.EvdLocked = CInt(DrEmpDetailsGO.Item("evd_locked"))

            cEmployeeGodOdm.plan1StartDate = DateTime.Parse(DrEmpDetailsGO("startDate1").ToString())
            cEmployeeGodOdm.plan1EndDate = DateTime.Parse(DrEmpDetailsGO("endDate1").ToString())
            cEmployeeGodOdm.plan1RadDate = DateTime.Parse(DrEmpDetailsGO("radniDate1").ToString())
            cEmployeeGodOdm.plan1Dani = CInt(DrEmpDetailsGO("brojDana1"))

            cEmployeeGodOdm.plan2StartDate = DateTime.Parse(DrEmpDetailsGO("startDate2").ToString())
            cEmployeeGodOdm.plan2EndDate = DateTime.Parse(DrEmpDetailsGO("endDate2").ToString())
            cEmployeeGodOdm.plan2RadDate = DateTime.Parse(DrEmpDetailsGO("radniDate2").ToString())
            cEmployeeGodOdm.plan2Dani = CInt(DrEmpDetailsGO("brojDana2"))

            cEmployeeGodOdm.DaniSGO = CInt(DrEmpDetailsGO("NeIskDani"))

            cEmployeeGodOdm.PreneseniSGO = CInt(DrEmpDetailsGO("NeIskDaniPreneseni"))
            cEmployeeGodOdm.PreneseniNGO = CInt(DrEmpDetailsGO("IzrDaniPreneseni"))

            cEmployeeGodOdm.AnulSGO = CInt(DrEmpDetailsGO("NeIskDaniAnul"))
            cEmployeeGodOdm.AnulNGO = CInt(DrEmpDetailsGO("IzrDaniAnul"))
        End If

        If DrEmpDetailsAGO IsNot Nothing AndAlso DrEmpDetailsAGO.Table.Rows.Count > 0 Then
            cEmployeeGodOdm.docMD5 = CStr(DrEmpDetailsAGO("doc_md5"))
        End If


        Return cEmployeeGodOdm
    End Function
End Class
