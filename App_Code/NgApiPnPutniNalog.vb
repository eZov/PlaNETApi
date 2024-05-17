Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Globalization
Imports System.IO
Imports System.Security.Cryptography.X509Certificates
Imports System.Web.Configuration
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json

''' <summary>
''' Objekat koji predstavlja evidencije prisustva. Evidencije predstavljaju jedan kalendarski mjesec.
''' </summary>
Public Class NgApiPnPutniNalog

    Public Sub New()

    End Sub

    Public Sub New(ByVal pEmployeeId As Integer, pStatusTxt As String,
                               Optional pDays As Integer = 30)

        getPnPutniNalog(pEmployeeId, pStatusTxt, pDays)

    End Sub

    Public Property PnPutniNalogList As List(Of ngPnPutniNalog)

    Public Property PnPutniNalogIzvDetList As List(Of NgPnPutniNalogDet)

    ' Detalji jednog putnog naloga koji se sastoje od neodreenog broja redova
    Public Property PnPutniNalogDet As New List(Of NgPnPutniNalogDet)

    ' Detalji izvještaja jednog putnog naloga koji se sastoje od neodreenog broja redova
    Public Property PnPutniNalogDetIzvDet As New List(Of NgPnPutniNalogDet)

    ' API L1
    ' byemp
    Public Sub getPnPutniNalog(ByVal pEmployeeId As Integer, pStatusTxt As String,
                               Optional pDays As Integer = 30)

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable


        strSQL = <![CDATA[ 
SELECT
 pn.`id`, 
 pn.`employee_id`, 
 pn.`ime_prezime`, 
 pn.radno_mjesto,
 pn.`mjesto_start`, 
 pn.`mjesto_putovanja`, 
 pn.`razlog_putovanja`,
 DATE_FORMAT(pn.`dat_poc_putovanja`,'%Y-%m-%d') AS dat_poc_putovanja,
 pn.`trajanje_putovanja`,
 pn.relacija,   
 pn.prev_text,
 pn.prev_auto AS PrevAuto,
 pn.prev_vlauto AS PrevVlAuto,
 pn.prev_avion AS PrevAvion,
 pn.prev_autobus AS PrevAutobus,
 pn.prevoz_voz AS PrevVoz, 
 pn.`troskovi_putovanja_knjizenje`,
 IFNULL(pn.aktivnost,'') AS aktivnost,   
 CONCAT(so.Naziv,' (',so.`Sifra`,')') AS nazivorgjed,
 pn.`org_jed`,
 IFNULL(pn.ukupnitroskovi_a,0) AS UkTroskoviA,
 IFNULL(pn.ukupnitroskovi_b,0) AS UkTroskoviB,
 IFNULL(pn.ukupnitroskovi_c,0) AS UkTroskoviC,
 IFNULL(pn.ukupnitroskovi_d,0) AS UkTroskoviD,
 IFNULL(pn.iznos_obracuna,0) AS UkIznosObracuna,
 IFNULL(pn.iznos_razlika,0) AS UkIznosRazlika,
 pn.iznos_dnevnice, 
 pn.proc_dnevnice, 
 pn.broj_dnevnica, 
  pn.akontacija_dnevnice, 
  pn.akontacija_nocenje,  
  pn.akontacija_ostalo, 
  pn.`iznos_akontacije`,
 pn.odobrena_akontacija,
 pn.vrsta_naloga,
 pns.`status_text` AS `status_naloga`,
 DATE_FORMAT(IFNULL(pno.status_time,'0001-01-01 00:00:00'),'%Y-%m-%dT%H:%i:%s') AS status_date, 
 CONVERT(IFNULL(pot.Potpis,'') USING cp1250) AS potpis_naloga
FROM
  putninalog pn INNER JOIN`putninalog_status` pns ON pn.`status_naloga`=pns.`id`
  INNER JOIN `evd_employees` e ON pn.`employee_id`=e.`EmployeeID`
  LEFT JOIN (SELECT
 pn.`id`, pns.`status_text`,
 GROUP_CONCAT(CONCAT(e.`LastName`,' ',e.`FirstName`)) AS Potpis
FROM
  putninalog pn 
  INNER JOIN`putninalog_status` pns ON pn.`status_naloga`=pns.`id`
  LEFT JOIN `putninalog_potpisi` pnp ON pn.`id`=pnp.`pn_id`
  LEFT JOIN `evd_potpisi` ep ON pnp.`evd_potpisi_id`=ep.`id`
  LEFT JOIN `evd_employees` e ON ep.`employee_id`=e.`EmployeeID`
WHERE pn.`employee_id`=@EmployeeId
GROUP BY pn.`id`) AS pot ON pn.`id`=pot.id
LEFT JOIN sfr_organizacija so ON e.DepartmentUP=so.id
LEFT JOIN (
SELECT
  pno.pn_id,
  MAX(pno.`dummytimestamp`) AS status_time
FROM
  `putninalog_odobravanje` pno
GROUP BY pno.`pn_id`
HAVING MAX(pno.`dummytimestamp`)) pno ON pno.pn_id=pn.`id`
WHERE pn.`employee_id`=@EmployeeId
AND  pn.`dat_poc_putovanja` > DATE_ADD(DATE(NOW()), INTERVAL -%__Days__% DAY)
AND pot.`status_text` IN(%__StatusText__%);
    ]]>.Value



        If pStatusTxt.Length = 0 Then pStatusTxt = "''"

        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            strSQL = strSQL.Replace("%__Days__%", pDays)
            strSQL = strSQL.Replace("%__StatusText__%", pStatusTxt)
            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@EmployeeId", pEmployeeId)
            mycmd.Prepare()


            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            'myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(DtList)

            'DtList.Rows.Add("post", "White", "Ukupno sati", "UKP", 0)

            Me.PnPutniNalogList = JsonConvert.DeserializeObject(Of List(Of ngPnPutniNalog))(GetJson(DtList))

        End Using

    End Sub

    ' API L1
    ' byuser
    Public Sub getPnUserPutniNalog(ByVal pEmail As String)

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable


        strSQL = <![CDATA[ 
SELECT
 pn.`id`, 
 pn.`employee_id`, 
 pn.`ime_prezime`, 
 pn.radno_mjesto,
 pn.`mjesto_start`, 
 pn.`mjesto_putovanja`, 
 pn.`razlog_putovanja`,
 DATE_FORMAT(pn.`dat_poc_putovanja`,'%Y-%m-%d') AS dat_poc_putovanja,
 pn.`trajanje_putovanja`,
 pn.relacija,
 pn.prev_text,   
 pn.prev_auto AS PrevAuto,
 pn.prev_vlauto AS PrevVlAuto,
 pn.prev_avion AS PrevAvion,
 pn.prev_autobus AS PrevAutobus,
 pn.prevoz_voz AS PrevVoz, 
 pn.`troskovi_putovanja_knjizenje`,
 IFNULL(pn.aktivnost,'') AS aktivnost,   
 CONCAT(so.Naziv,' (',so.`Sifra`,')') AS nazivorgjed,
 pn.`org_jed`,
 IFNULL(pn.ukupnitroskovi_a,0) AS UkTroskoviA,
 IFNULL(pn.ukupnitroskovi_b,0) AS UkTroskoviB,
 IFNULL(pn.ukupnitroskovi_c,0) AS UkTroskoviC,
 IFNULL(pn.ukupnitroskovi_d,0) AS UkTroskoviD,
 IFNULL(pn.iznos_obracuna,0) AS UkIznosObracuna,
 IFNULL(pn.iznos_razlika,0) AS UkIznosRazlika,
 pn.iznos_dnevnice, 
 pn.proc_dnevnice, 
 pn.broj_dnevnica, 
  pn.akontacija_dnevnice, 
  pn.akontacija_nocenje,  
  pn.akontacija_ostalo, 
  pn.`iznos_akontacije`,
  pn.`ispl_akontacije`,
 pn.odobrena_akontacija,
 pn.vrsta_naloga,
 pns.`status_text` AS `status_naloga`,
 CONVERT(IFNULL(pot.Potpis,'') USING cp1250) AS potpis_naloga
FROM
  putninalog pn INNER JOIN`putninalog_status` pns ON pn.`status_naloga`=pns.`id`
  INNER JOIN `evd_employees` e ON pn.`employee_id`=e.`EmployeeID`
  INNER JOIN `putninalog_user` pnu ON pn.`id`=pnu.`pn_id`
  INNER JOIN `my_aspnet_membership` m ON pnu.`user_id`=m.`userId`+1  
  LEFT JOIN (SELECT
 pn.`id`, 
 GROUP_CONCAT(CONCAT(e.`LastName`,' ',e.`FirstName`)) AS Potpis
FROM
  putninalog pn 
  INNER JOIN`putninalog_status` pns ON pn.`status_naloga`=pns.`id`
  INNER JOIN `putninalog_user` pnu ON pn.`id`=pnu.`pn_id`
  INNER JOIN `my_aspnet_membership` m ON pnu.`user_id`=m.`userId`+1  
  LEFT JOIN `putninalog_potpisi` pnp ON pn.`id`=pnp.`pn_id`
  LEFT JOIN `evd_potpisi` ep ON pnp.`evd_potpisi_id`=ep.`id`
  LEFT JOIN `evd_employees` e ON ep.`employee_id`=e.`EmployeeID`
WHERE m.`Email`=@Email
GROUP BY pn.`id`) AS pot ON pn.`id`=pot.id
LEFT JOIN sfr_organizacija so ON e.DepartmentUP=so.id
WHERE m.`Email`=@Email;
    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@Email", pEmail)
            mycmd.Prepare()

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            'myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(DtList)

            'DtList.Rows.Add("post", "White", "Ukupno sati", "UKP", 0)

            Me.PnPutniNalogList = JsonConvert.DeserializeObject(Of List(Of ngPnPutniNalog))(GetJson(DtList))

        End Using

    End Sub

    ' API L2
    ' byorg
    Public Sub getPnOrgPutniNalog(ByVal pLstOrg As String, ByVal pTmpTableName As String, ByVal pStatusTxt As String,
                                  ByVal pEmployeeId As Integer,
                                  Optional pDays As Integer = 30,
                                  Optional pCreatedBy As Integer = -1,
                                  Optional pExtEmployees As String = "",
                                   Optional pVrsta As String = "")

        '
        ' Uzmi org.id iz sys_usr_role_json tabele za usera koji ima email: my_aspnet_membership.Email
        '
        'Dim _lstOrg As String = getOrgJed(pEmail)


        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable


        strSQL = <![CDATA[ 
DROP TABLE IF EXISTS %__tmp_epotpisi__%;

CREATE TEMPORARY TABLE `%__tmp_epotpisi__%` (
  `id` INT(11) NOT NULL DEFAULT '0',
  `isSignee` INT(11) NOT NULL DEFAULT '0',
  `Potpis` LONGTEXT CHARACTER SET cp1250 NOT NULL,
  status_date  DATETIME DEFAULT NULL,
  created_by  VARCHAR(100) CHARACTER SET cp1250 DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=INNODB DEFAULT CHARSET=latin1;

INSERT IGNORE INTO %__tmp_epotpisi__%
SELECT
 pn.`id`, 
 0 AS isSignee,
 IFNULL(GROUP_CONCAT(CONCAT(e.`LastName`,' ',e.`FirstName`) ORDER BY pnp.`id` ASC),'') AS Potpis
 ,NULL, NULL
FROM
  putninalog pn 
  INNER JOIN`putninalog_status` pns ON pn.`status_naloga`=pns.`id`
  INNER JOIN `putninalog_user` pnu ON pn.`id`=pnu.`pn_id`
  INNER JOIN `evd_employees` e1 ON pn.`employee_id`=e1.`EmployeeID`
  LEFT JOIN `putninalog_potpisi` pnp ON pn.`id`=pnp.`pn_id`
  LEFT JOIN `evd_potpisi` ep ON pnp.`evd_potpisi_id`=ep.`id`
  LEFT JOIN `evd_employees` e ON ep.`employee_id`=e.`EmployeeID`
WHERE e1.`DepartmentUP` IN(SELECT so.`id` 
FROM `sfr_organizacija` so
WHERE so.`Sifra` IN(%__ListOrgJed__%))
AND  pn.`dat_poc_putovanja` > DATE_ADD(DATE(NOW()), INTERVAL -%__Days__% DAY)
GROUP BY pn.`id`;

UPDATE   %__tmp_epotpisi__% pn 
  INNER JOIN `putninalog_potpisi` pnp ON pn.`id`=pnp.`pn_id`
  INNER JOIN `evd_potpisi` ep ON pnp.`evd_potpisi_id`=ep.`id`
  INNER JOIN `evd_employees` e ON ep.`employee_id`=e.`EmployeeID`
SET pn.isSignee = 1
WHERE e.`EmployeeID`=%__EmployeeId__%;

UPDATE   %__tmp_epotpisi__% pn 
  INNER JOIN `putninalog_potpisi` pnp ON pn.`id`=pnp.`pn_id`
  INNER JOIN `evd_potpisi` ep ON pnp.`evd_potpisi_id`=ep.`id`
  INNER JOIN `evd_potpisi_status` eps ON ep.`id`=eps.`id`
  INNER JOIN `evd_employees` e ON ep.`employee_id`=e.`EmployeeID`
SET pn.isSignee = 2
WHERE e.`EmployeeID`=%__EmployeeId__%;


UPDATE    %__tmp_epotpisi__% t1, 
(
SELECT pno.pn_id AS pn_id
,MAX(pno.dummytimestamp) AS dt
FROM putninalog_odobravanje pno
LEFT JOIN putninalog pn ON pno.pn_id=pn.id
AND  pn.dat_poc_putovanja > DATE_ADD(DATE(NOW()), INTERVAL -%__Days__% DAY)
GROUP BY pno.pn_id
ORDER BY pno.pn_id ASC
) t2
SET t1.status_date = t2.dt
WHERE t1.id = t2.pn_id;

UPDATE  %__tmp_epotpisi__% pn 
INNER JOIN putninalog_user pnu ON pnu.pn_id=pn.id 
LEFT JOIN my_aspnet_users mu ON pnu.user_id=mu.id 
LEFT JOIN my_aspnet_users_to_employees u2e ON u2e.users_id+1=mu.id 
LEFT JOIN evd_employees e ON u2e.employees_id=e.EmployeeID 
SET pn.created_by = CONCAT(e.FirstName,' ',e.LastName); 

DROP TABLE IF EXISTS %__tmp_epotpisi__%_tem;  
  CREATE TEMPORARY TABLE %__tmp_epotpisi__%_tem
  SELECT 
pnt1.pn_id, 
IFNULL(pnt1.pn_temeljnica,'') AS temeljnica_1,
IFNULL(pnt2.pn_temeljnica,'') AS temeljnica_2
FROM putninalog_temeljnica pnt1
INNER JOIN putninalog_temeljnica pnt2 ON pnt1.pn_id=pnt2.pn_id,
%__tmp_epotpisi__% t
WHERE pnt1.pn_vrsta=1 
AND pnt2.pn_vrsta=2
AND t.id=pnt1.pn_id 
AND  t.id=pnt1.pn_id;

SELECT
 pn.`id`, 
 pn.`employee_id`, 
 pn.`ime_prezime`, 
 pn.radno_mjesto,
 pn.`mjesto_start`, 
 pn.`mjesto_putovanja`, 
 pn.`razlog_putovanja`,
 DATE_FORMAT(pn.`dat_poc_putovanja`,'%Y-%m-%d') AS dat_poc_putovanja,
 pn.`trajanje_putovanja`,
 pn.relacija,   
 pn.prev_text,
 pn.prev_auto AS PrevAuto,
 pn.prev_vlauto AS PrevVlAuto,
 pn.prev_avion AS PrevAvion,
 pn.prev_autobus AS PrevAutobus,
 pn.prevoz_voz AS PrevVoz, 
 pn.`troskovi_putovanja_knjizenje`,
 IFNULL(pn.aktivnost,'') AS aktivnost,   
 CONCAT(so.Naziv,' (',so.`Sifra`,')') AS nazivorgjed,
 pn.`org_jed`,
 IFNULL(pn.ukupnitroskovi_a,0) AS UkTroskoviA,
 IFNULL(pn.ukupnitroskovi_b,0) AS UkTroskoviB,
 IFNULL(pn.ukupnitroskovi_c,0) AS UkTroskoviC,
 IFNULL(pn.ukupnitroskovi_d,0) AS UkTroskoviD,
 IFNULL(pn.iznos_obracuna,0) AS UkIznosObracuna,
 IFNULL(pn.iznos_razlika,0) AS UkIznosRazlika,
 pn.iznos_dnevnice, 
 pn.proc_dnevnice, 
 pn.broj_dnevnica, 
  pn.akontacija_dnevnice, 
  pn.akontacija_nocenje,  
  pn.akontacija_ostalo, 
  pn.`iznos_akontacije`,
  pn.`ispl_akontacije`,
 pn.odobrena_akontacija,
 pn.vrsta_naloga,
 pns.`status_text` AS `status_naloga`,
 DATE_FORMAT(IFNULL(pot.status_date,'0001-01-01 00:00:00'),'%Y-%m-%dT%H:%i:%s') AS status_date, 
 pot.created_by, 
 CONVERT(IFNULL(pot.Potpis,'') USING cp1250) AS potpis_naloga,
  pot.isSignee,
IFNULL(pnt.temeljnica_1,'') AS temeljnica_1,
IFNULL(pnt.temeljnica_2,'') AS temeljnica_2,
    (
    CASE 
        WHEN pe.`pn_protokol` IS NULL THEN ''
        ELSE CONCAT(pe.`pn_protokol`, '/',pe.`pn_redbr`, '-',pe.`pn_mjesec`, '/',pe.`pn_godina`)
    END) AS pn_protokol
FROM putninalog pn 
INNER JOIN`putninalog_status` pns ON pn.`status_naloga`=pns.`id`
INNER JOIN `evd_employees` e ON pn.`employee_id`=e.`EmployeeID`
INNER JOIN %__tmp_epotpisi__% AS pot ON pn.`id`=pot.id
LEFT JOIN sfr_organizacija so ON e.DepartmentUP=so.id
LEFT JOIN putninalog_user pnu ON pn.id=pnu.pn_id
LEFT JOIN my_aspnet_users_to_employees u2e ON pnu.user_id-1=u2e.users_id
LEFT JOIN  %__tmp_epotpisi__%_tem
pnt on pn.id=pnt.pn_id
LEFT JOIN `putninalog_evidencija` pe ON pn.`id` = pe.`pn_id`
WHERE pns.`status_text` IN(%__StatusText__%)
%__CreatedBy__%
%__ExtEmployees__%
%__Vrsta__%
ORDER BY pn.`dat_poc_putovanja`;

    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            strSQL = strSQL.Replace("%__tmp_epotpisi__%", pTmpTableName)
            strSQL = strSQL.Replace("%__ListOrgJed__%", pLstOrg)
            strSQL = strSQL.Replace("%__Days__%", pDays)
            strSQL = strSQL.Replace("%__StatusText__%", pStatusTxt)
            strSQL = strSQL.Replace("%__EmployeeId__%", pEmployeeId)

            strSQL = IIf(pCreatedBy > 0, strSQL.Replace("%__CreatedBy__%", " AND u2e.employees_id = " + pCreatedBy.ToString),
                         strSQL.Replace("%__CreatedBy__%", ""))
            strSQL = IIf(pExtEmployees <> "", strSQL.Replace("%__ExtEmployees__%", " AND e.Title IN ('" + pExtEmployees + "')"), strSQL.Replace("%__ExtEmployees__%", ""))
            strSQL = IIf(pVrsta <> "", strSQL.Replace("%__Vrsta__%", " AND pn.vrsta_naloga IN (" + pVrsta + ")"), strSQL.Replace("%__Vrsta__%", ""))

            mycmd.CommandText = strSQL


            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)


            Me.PnPutniNalogList = JsonConvert.DeserializeObject(Of List(Of ngPnPutniNalog))(GetJson(DtList))

        End Using

    End Sub

    ' API L2
    ' bypg
    Public Sub getPnPGPutniNalog(ByVal pLstOrg As String, ByVal pTmpTableName As String, ByVal pStatusTxt As String,
                                 ByVal pEmployeeId As Integer,
                                 Optional pDays As Integer = 30,
                                  Optional pCreatedBy As Integer = -1,
                                  Optional pExtEmployees As String = "",
                                   Optional pVrsta As String = "",
                                  Optional pPnDoc As Integer = -1)

        '
        ' Uzmi org.id iz sys_usr_role_json tabele za usera koji ima email: my_aspnet_membership.Email
        '
        'Dim _lstOrg As String = getOrgJed(pEmail)


        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable



        strSQL = <![CDATA[ 
DROP TABLE IF EXISTS %__tmp_epotpisi__%;

CREATE TEMPORARY TABLE `%__tmp_epotpisi__%` (
  `id` INT(11) NOT NULL DEFAULT '0',
  `isSignee` INT(11) NOT NULL DEFAULT '0',
  `Potpis` LONGTEXT CHARACTER SET cp1250 NOT NULL,
  status_date  DATETIME DEFAULT NULL,
  created_by  VARCHAR(100) CHARACTER SET cp1250 DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=INNODB DEFAULT CHARSET=latin1;

INSERT IGNORE INTO %__tmp_epotpisi__%
SELECT
 pn.`id`, 
 0 AS isSignee,
 IFNULL(GROUP_CONCAT(CONCAT(e.`LastName`,' ',e.`FirstName`) ORDER BY pnp.`id` ASC),'') AS Potpis
 ,NULL, NULL
FROM
  putninalog pn 
  INNER JOIN`putninalog_status` pns ON pn.`status_naloga`=pns.`id`
  INNER JOIN `putninalog_user` pnu ON pn.`id`=pnu.`pn_id`
  INNER JOIN `evd_employees` e1 ON pn.`employee_id`=e1.`EmployeeID`
  LEFT JOIN `putninalog_potpisi` pnp ON pn.`id`=pnp.`pn_id` AND pnp.pn_doc = @pn_doc
  LEFT JOIN `evd_potpisi` ep ON pnp.`evd_potpisi_id`=ep.`id`
  LEFT JOIN `evd_employees` e ON ep.`employee_id`=e.`EmployeeID`
WHERE  e1.`EmployeeID`  IN(%__ListOrgJed__%)
AND  pn.`dat_poc_putovanja` > DATE_ADD(DATE(NOW()), INTERVAL -%__Days__% DAY)
GROUP BY pn.`id`;

UPDATE   %__tmp_epotpisi__% pn 
  INNER JOIN `putninalog_potpisi` pnp ON pn.`id`=pnp.`pn_id`
  INNER JOIN `evd_potpisi` ep ON pnp.`evd_potpisi_id`=ep.`id`
  INNER JOIN `evd_employees` e ON ep.`employee_id`=e.`EmployeeID`
SET pn.isSignee = 1
WHERE e.`EmployeeID`=%__EmployeeId__%;

UPDATE   %__tmp_epotpisi__% pn 
  INNER JOIN `putninalog_potpisi` pnp ON pn.`id`=pnp.`pn_id`
  INNER JOIN `evd_potpisi` ep ON pnp.`evd_potpisi_id`=ep.`id`
  INNER JOIN `evd_potpisi_status` eps ON ep.`id`=eps.`id`
  INNER JOIN `evd_employees` e ON ep.`employee_id`=e.`EmployeeID`
SET pn.isSignee = 2
WHERE e.`EmployeeID`=%__EmployeeId__%;


UPDATE    %__tmp_epotpisi__% t1, 
(
SELECT pno.pn_id AS pn_id
,MAX(pno.dummytimestamp) AS dt
FROM putninalog_odobravanje pno
LEFT JOIN putninalog pn ON pno.pn_id=pn.id
AND  pn.dat_poc_putovanja > DATE_ADD(DATE(NOW()), INTERVAL -%__Days__% DAY)
GROUP BY pno.pn_id
ORDER BY pno.pn_id ASC
) t2
SET t1.status_date = t2.dt
WHERE t1.id = t2.pn_id;

UPDATE  %__tmp_epotpisi__% pn 
INNER JOIN putninalog_user pnu ON pnu.pn_id=pn.id 
LEFT JOIN my_aspnet_users mu ON pnu.user_id=mu.id 
LEFT JOIN my_aspnet_users_to_employees u2e ON u2e.users_id+1=mu.id 
LEFT JOIN evd_employees e ON u2e.employees_id=e.EmployeeID 
SET pn.created_by = CONCAT(e.FirstName,' ',e.LastName); 

DROP TABLE IF EXISTS %__tmp_epotpisi__%_tem;  
  CREATE TEMPORARY TABLE %__tmp_epotpisi__%_tem
  SELECT 
pnt1.pn_id, 
IFNULL(pnt1.pn_temeljnica,'') AS temeljnica_1,
IFNULL(pnt2.pn_temeljnica,'') AS temeljnica_2
FROM putninalog_temeljnica pnt1
INNER JOIN putninalog_temeljnica pnt2 ON pnt1.pn_id=pnt2.pn_id,
%__tmp_epotpisi__% t
WHERE pnt1.pn_vrsta=1 
AND pnt2.pn_vrsta=2
AND t.id=pnt1.pn_id 
AND  t.id=pnt1.pn_id;


SELECT
 pn.`id`, 
 pn.`employee_id`, 
 pn.`ime_prezime`, 
 pn.radno_mjesto,
 pn.`mjesto_start`, 
 pn.`mjesto_putovanja`, 
 pn.`razlog_putovanja`,
 DATE_FORMAT(pn.`dat_poc_putovanja`,'%Y-%m-%d') AS dat_poc_putovanja,
 pn.`trajanje_putovanja`,
 pn.relacija,   
 pn.prev_text,  
 pn.prev_auto AS PrevAuto,
 pn.prev_vlauto AS PrevVlAuto,
 pn.prev_avion AS PrevAvion,
 pn.prev_autobus AS PrevAutobus,
 pn.prevoz_voz AS PrevVoz, 
 pn.`troskovi_putovanja_knjizenje`,
 IFNULL(pn.aktivnost,'') AS aktivnost,   
 CONCAT(so.Naziv,' (',so.`Sifra`,')') AS nazivorgjed,
 pn.`org_jed`,
 IFNULL(pn.ukupnitroskovi_a,0) AS UkTroskoviA,
 IFNULL(pn.ukupnitroskovi_b,0) AS UkTroskoviB,
 IFNULL(pn.ukupnitroskovi_c,0) AS UkTroskoviC,
 IFNULL(pn.ukupnitroskovi_d,0) AS UkTroskoviD,
 IFNULL(pn.iznos_obracuna,0) AS UkIznosObracuna,
 IFNULL(pn.iznos_razlika,0) AS UkIznosRazlika,
 pn.iznos_dnevnice, 
 pn.proc_dnevnice,
 pn.broj_dnevnica, 
  pn.akontacija_dnevnice, 
  pn.akontacija_nocenje,  
  pn.akontacija_ostalo, 
  pn.`iznos_akontacije`,
  pn.`ispl_akontacije`,
 pn.odobrena_akontacija,
 pn.vrsta_naloga,
 pns.`status_text` AS `status_naloga`,
 DATE_FORMAT(IFNULL(pot.status_date,'0001-01-01 00:00:00'),'%Y-%m-%dT%H:%i:%s') AS status_date, 
 pot.created_by, 
 CONVERT(IFNULL(pot.Potpis,'') USING cp1250) AS potpis_naloga,
  pot.isSignee,
IFNULL(pnt.temeljnica_1,'') AS temeljnica_1,
IFNULL(pnt.temeljnica_2,'') AS temeljnica_2
FROM putninalog pn 
INNER JOIN`putninalog_status` pns ON pn.`status_naloga`=pns.`id`
INNER JOIN `evd_employees` e ON pn.`employee_id`=e.`EmployeeID`
INNER JOIN %__tmp_epotpisi__% AS pot ON pn.`id`=pot.id
LEFT JOIN sfr_organizacija so ON e.DepartmentUP=so.id
LEFT JOIN putninalog_user pnu ON pn.id=pnu.pn_id
LEFT JOIN my_aspnet_users_to_employees u2e ON pnu.user_id-1=u2e.users_id
LEFT JOIN  %__tmp_epotpisi__%_tem
pnt on pn.id=pnt.pn_id
WHERE pns.`status_text` IN(%__StatusText__%)
%__CreatedBy__%
%__ExtEmployees__%
%__Vrsta__%
ORDER BY pn.`dat_poc_putovanja`;

    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            strSQL = strSQL.Replace("%__tmp_epotpisi__%", pTmpTableName)
            strSQL = strSQL.Replace("%__ListOrgJed__%", pLstOrg)
            strSQL = strSQL.Replace("%__Days__%", pDays)
            strSQL = strSQL.Replace("%__StatusText__%", pStatusTxt)
            strSQL = strSQL.Replace("%__EmployeeId__%", pEmployeeId)

            strSQL = IIf(pCreatedBy > 0, strSQL.Replace("%__CreatedBy__%", " AND u2e.employees_id = " + pCreatedBy.ToString), strSQL.Replace("%__CreatedBy__%", ""))
            strSQL = IIf(pExtEmployees <> "", strSQL.Replace("%__ExtEmployees__%", " AND e.Title IN ('" + pExtEmployees + "')"), strSQL.Replace("%__ExtEmployees__%", ""))
            strSQL = IIf(pVrsta <> "", strSQL.Replace("%__Vrsta__%", " AND pn.vrsta_naloga IN (" + pVrsta + ")"), strSQL.Replace("%__Vrsta__%", ""))

            mycmd.CommandText = strSQL


            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)


            Me.PnPutniNalogList = JsonConvert.DeserializeObject(Of List(Of ngPnPutniNalog))(GetJson(DtList))

        End Using

    End Sub

    ' API L2
    ' byorg + bypg
    Public Sub getPnOrgPGPutniNalog(ByVal pLstOrg As String, ByVal pLstPG As String, ByVal pTmpTableName As String, ByVal pStatusTxt As String,
                                     ByVal pEmployeeId As Integer,
                                    Optional pDays As Integer = 30,
                                  Optional pCreatedBy As Integer = -1,
                                  Optional pExtEmployees As String = "",
                                   Optional pVrsta As String = "",
                                  Optional pPnDoc As Integer = -1)
        '
        ' Uzmi org.id iz sys_usr_role_json tabele za usera koji ima email: my_aspnet_membership.Email
        '
        'Dim _lstOrg As String = getOrgJed(pEmail)


        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL, strSQLStart, strSQLOrg, strSQLPG, strSQLEnd, strSQLOrgSar As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQLStart = <![CDATA[ 
DROP TABLE IF EXISTS %__tmp_epotpisi__%;

CREATE TEMPORARY TABLE %__tmp_epotpisi__% (
  id INT(11) NOT NULL DEFAULT '0',
  isSignee INT(11) NOT NULL DEFAULT '0',
  PotpisId TEXT CHARACTER SET cp1250 NOT NULL, 
  Potpis LONGTEXT CHARACTER SET cp1250 NOT NULL,
  status_date  DATETIME DEFAULT NULL,
  created_by  VARCHAR(100) CHARACTER SET cp1250 DEFAULT NULL,
  PRIMARY KEY (id)
) ENGINE=INNODB DEFAULT CHARSET=latin1;

    ]]>.Value

        strSQLOrg = <![CDATA[ 
DROP TABLE IF EXISTS %__tmp_epotpisi__%_orgid;

CREATE TEMPORARY TABLE %__tmp_epotpisi__%_orgid (
  `id` INT(11) NOT NULL DEFAULT '0'
) ENGINE=INNODB DEFAULT CHARSET=latin1;

INSERT IGNORE INTO %__tmp_epotpisi__%_orgid
SELECT
  so.id
FROM
  sfr_organizacija so
WHERE so.Sifra IN(%__ListOrgJed__%);




INSERT IGNORE INTO %__tmp_epotpisi__%
SELECT
 pn.id, 
 IFNULL(MAX(pnp.id), -1) AS isSignee,
 IFNULL(GROUP_CONCAT(e.EmployeeID ORDER BY pnp.`id` ASC),'') AS PotpisId,  
 IFNULL(GROUP_CONCAT(CONCAT(e.LastName,' ',e.FirstName) ORDER BY pnp.`id` ASC),'') AS Potpis
 ,NULL, NULL
FROM
  putninalog pn 
  INNER JOIN putninalog_status pns ON pn.status_naloga=pns.id
  INNER JOIN putninalog_user pnu ON pn.id=pnu.pn_id
  INNER JOIN evd_employees e1 ON pn.employee_id=e1.EmployeeID
  LEFT JOIN putninalog_potpisi pnp ON pn.id=pnp.pn_id  %__pn_doc__%
  LEFT JOIN evd_potpisi ep ON pnp.evd_potpisi_id=ep.id
  LEFT JOIN evd_employees e ON ep.employee_id=e.EmployeeID,
  %__tmp_epotpisi__%_orgid o
WHERE e1.DepartmentUP = o.id
AND e1.Aktivan <> 0
AND  pn.dat_poc_putovanja > DATE_ADD(DATE(NOW()), INTERVAL -%__Days__% DAY)
GROUP BY pn.id;
    ]]>.Value

        strSQLPG = <![CDATA[ 
INSERT IGNORE INTO %__tmp_epotpisi__%
SELECT
 pn.id, 
 IFNULL(MAX(pnp.id), -1) AS isSignee,
 IFNULL(GROUP_CONCAT(e.EmployeeID ORDER BY pnp.`id` ASC),'') AS PotpisId,  
 IFNULL(GROUP_CONCAT(CONCAT(e.LastName,' ',e.FirstName)),'') AS Potpis
 ,NULL, NULL
FROM
  putninalog pn 
  INNER JOIN putninalog_status pns ON pn.status_naloga=pns.id
  INNER JOIN putninalog_user pnu ON pn.id=pnu.pn_id
  INNER JOIN evd_employees e1 ON pn.employee_id=e1.EmployeeID
  LEFT JOIN putninalog_potpisi pnp ON pn.id=pnp.pn_id  %__pn_doc__%
  LEFT JOIN evd_potpisi ep ON pnp.evd_potpisi_id=ep.id
  LEFT JOIN evd_employees e ON ep.employee_id=e.EmployeeID
WHERE  e1.EmployeeID  IN(%__ListPGJed__%)
AND  pn.dat_poc_putovanja > DATE_ADD(DATE(NOW()), INTERVAL -%__Days__% DAY)
GROUP BY pn.id;

INSERT IGNORE INTO %__tmp_epotpisi__%
SELECT
 pn.id, 
 IFNULL(MAX(pnp.id), -1) AS isSignee,
 IFNULL(GROUP_CONCAT(e.`sifra` ORDER BY pnp.`id` ASC),'') AS PotpisId,  
 IFNULL(GROUP_CONCAT(CONCAT(e.`prezime`,' ',e.`ime`)),'') AS Potpis
 ,NULL, NULL
FROM
  putninalog pn 
  INNER JOIN putninalog_status pns ON pn.status_naloga=pns.id
  INNER JOIN putninalog_user pnu ON pn.id=pnu.pn_id
  INNER JOIN evd_saradnici s1 ON pn.employee_id = s1.`sifra`
  LEFT JOIN `evd_employees` es ON s1.`sifra`=es.`EmployeeID`  
  LEFT JOIN putninalog_potpisi pnp ON pn.id=pnp.pn_id  %__pn_doc__%
  LEFT JOIN evd_potpisi ep ON pnp.evd_potpisi_id=ep.id
  LEFT JOIN `evd_saradnici` e ON ep.employee_id = e.`sifra`
WHERE  s1.`sifra`  IN(%__ListPGJed__%)
AND (es.`EmployeeID` IS NULL OR es.`Aktivan` = 0) 
AND  pn.dat_poc_putovanja > DATE_ADD(DATE(NOW()), INTERVAL -%__Days__% DAY)
GROUP BY pn.id;  
    ]]>.Value


        strSQLEnd = <![CDATA[ 
UPDATE   %__tmp_epotpisi__% pn 
  INNER JOIN putninalog_potpisi pnp ON pn.id=pnp.pn_id  %__pn_doc__%
  INNER JOIN evd_potpisi ep ON pnp.evd_potpisi_id=ep.id
  INNER JOIN evd_employees e ON ep.employee_id=e.EmployeeID
SET pn.isSignee = -999
WHERE e.EmployeeID=%__EmployeeId__%
AND pnp.id = pn.`isSignee`;

UPDATE   %__tmp_epotpisi__% pn 
SET pn.isSignee = 0
WHERE pn.isSignee > 0;

UPDATE   %__tmp_epotpisi__% pn 
SET pn.isSignee = 1
WHERE pn.isSignee = -999;

UPDATE   %__tmp_epotpisi__% pn 
SET pn.isSignee = 0
WHERE pn.isSignee < 0;

UPDATE    %__tmp_epotpisi__% t1, 
(
SELECT pno.pn_id AS pn_id
,MAX(pno.dummytimestamp) AS dt
FROM putninalog_odobravanje pno
LEFT JOIN putninalog pn ON pno.pn_id=pn.id
AND  pn.dat_poc_putovanja > DATE_ADD(DATE(NOW()), INTERVAL -%__Days__% DAY)
GROUP BY pno.pn_id
ORDER BY pno.pn_id ASC
) t2
SET t1.status_date = t2.dt
WHERE t1.id = t2.pn_id;

UPDATE  %__tmp_epotpisi__% pn 
INNER JOIN putninalog_user pnu ON pnu.pn_id=pn.id 
LEFT JOIN my_aspnet_users mu ON pnu.user_id=mu.id 
LEFT JOIN my_aspnet_users_to_employees u2e ON u2e.users_id+1=mu.id 
LEFT JOIN evd_employees e ON u2e.employees_id=e.EmployeeID 
SET pn.created_by = CONCAT(e.FirstName,' ',e.LastName); 

DROP TABLE IF EXISTS %__tmp_epotpisi__%_tem;  
  CREATE TEMPORARY TABLE %__tmp_epotpisi__%_tem
  SELECT 
pnt1.pn_id, 
IFNULL(pnt1.pn_temeljnica,'') AS temeljnica_1,
IFNULL(pnt2.pn_temeljnica,'') AS temeljnica_2
FROM putninalog_temeljnica pnt1
INNER JOIN putninalog_temeljnica pnt2 ON pnt1.pn_id=pnt2.pn_id,
%__tmp_epotpisi__% t
WHERE pnt1.pn_vrsta=1 
AND pnt2.pn_vrsta=2
AND t.id=pnt1.pn_id 
AND  t.id=pnt1.pn_id;

SELECT
 pn.id, 
 pn.employee_id, 
 pn.ime_prezime, 
 pn.radno_mjesto,
 pn.mjesto_start, 
 pn.mjesto_putovanja, 
 pn.razlog_putovanja,
 DATE_FORMAT(pn.dat_poc_putovanja,'%Y-%m-%d') AS dat_poc_putovanja,
 pn.trajanje_putovanja,
 pn.relacija,   
 pn.prev_text,  
 pn.prev_auto AS PrevAuto,
 pn.prev_vlauto AS PrevVlAuto,
 pn.prev_avion AS PrevAvion,
 pn.prev_autobus AS PrevAutobus,
 pn.prevoz_voz AS PrevVoz, 
 pn.troskovi_putovanja_knjizenje,
 IFNULL(pn.aktivnost,'') AS aktivnost,   
 IFNULL(CONCAT(so.Naziv,' (',so.Sifra,')'),'') AS nazivorgjed,
 pn.org_jed,
 IFNULL(pn.ukupnitroskovi_a,0) AS UkTroskoviA,
 IFNULL(pn.ukupnitroskovi_b,0) AS UkTroskoviB,
 IFNULL(pn.ukupnitroskovi_c,0) AS UkTroskoviC,
 IFNULL(pn.ukupnitroskovi_d,0) AS UkTroskoviD,
 IFNULL(pn.iznos_obracuna,0) AS UkIznosObracuna,
 IFNULL(pn.iznos_razlika,0) AS UkIznosRazlika,
 pn.iznos_dnevnice, 
 pn.proc_dnevnice, 
 pn.broj_dnevnica, 
  pn.akontacija_dnevnice, 
  pn.akontacija_nocenje,  
  pn.akontacija_ostalo, 
  pn.iznos_akontacije,
  pn.`ispl_akontacije`,
 pn.odobrena_akontacija,
 pn.vrsta_naloga,
 pns.status_text AS status_naloga,
 DATE_FORMAT(IFNULL(pot.status_date,'0001-01-01 00:00:00'),'%Y-%m-%dT%H:%i:%s') AS status_date, 
 pot.created_by, 
 CONVERT(IFNULL(pot.Potpis,'') USING cp1250) AS potpis_naloga,
 pot.isSignee,
IFNULL(pnt.temeljnica_1,'') AS temeljnica_1,
IFNULL(pnt.temeljnica_2,'') AS temeljnica_2,
    (
    CASE 
        WHEN pe.`pn_protokol` IS NULL THEN '...'
        ELSE CONCAT(pe.`pn_protokol`, '/',pe.`pn_redbr`, '-',pe.`pn_mjesec`, '/',pe.`pn_godina`)
    END) AS pn_protokol,
IFNULL(e.`Aktivan`, 0) AS uposlenik    
FROM putninalog pn 
INNER JOIN putninalog_status pns ON pn.status_naloga=pns.id
LEFT JOIN evd_employees e ON pn.employee_id=e.EmployeeID
INNER JOIN %__tmp_epotpisi__% AS pot ON pn.id=pot.id
LEFT JOIN sfr_organizacija so ON e.DepartmentUP=so.id
LEFT JOIN putninalog_user pnu ON pn.id=pnu.pn_id
LEFT JOIN my_aspnet_users_to_employees u2e ON pnu.user_id-1=u2e.users_id
LEFT JOIN  %__tmp_epotpisi__%_tem
pnt on pn.id=pnt.pn_id
LEFT JOIN `putninalog_evidencija` pe ON pn.`id` = pe.`pn_id`
WHERE pns.status_text IN(%__StatusText__%)
%__CreatedBy__%
%__ExtEmployees__%
%__Vrsta__%
ORDER BY pn.dat_poc_putovanja;

    ]]>.Value

        strSQLOrgSar = <![CDATA[ 
INSERT IGNORE INTO %__tmp_epotpisi__%
SELECT
 pn.id, 
 IFNULL(MAX(pnp.id), -1) AS isSignee,
 IFNULL(GROUP_CONCAT(e.EmployeeID ORDER BY pnp.`id` ASC),'') AS PotpisId,  
 IFNULL(GROUP_CONCAT(CONCAT(e.LastName,' ',e.FirstName) ORDER BY pnp.`id` ASC),'') AS Potpis
 ,NULL, NULL
FROM
  putninalog pn 
  INNER JOIN putninalog_status pns ON pn.status_naloga=pns.id
  INNER JOIN putninalog_user pnu ON pn.id=pnu.pn_id
  INNER JOIN `evd_saradnici` s ON pn.employee_id=s.`sifra`
  LEFT JOIN `evd_employees` es ON s.`sifra`=es.`EmployeeID`
  LEFT JOIN putninalog_potpisi pnp ON pn.id=pnp.pn_id  %__pn_doc__%
  LEFT JOIN evd_potpisi ep ON pnp.evd_potpisi_id=ep.id
  LEFT JOIN evd_employees e ON ep.employee_id=e.EmployeeID
WHERE 
(es.`EmployeeID` IS NULL OR es.`Aktivan` = 0) 
AND  pn.dat_poc_putovanja > DATE_ADD(DATE(NOW()), INTERVAL -%__Days__% DAY)
GROUP BY pn.id; 
    ]]>.Value

        strSQL = <![CDATA[ 
    ]]>.Value

        Dim pLstOrgToken As String() = pLstOrg.Split(","c)
        'If pLstOrgToken.Contains("1") Then
        '    strSQLOrg = strSQLOrg + strSQLOrgSar
        'End If

        If pLstOrg.Length > 0 AndAlso pLstPG.Length > 0 Then
            strSQL = strSQLStart + strSQLOrg + strSQLPG + strSQLEnd
        ElseIf pLstOrg.Length > 0 AndAlso pLstPG.Length = 0 Then
            strSQL = strSQLStart + strSQLOrg + strSQLEnd
        ElseIf pLstOrg.Length = 0 AndAlso pLstPG.Length > 0 Then
            strSQL = strSQLStart + strSQLPG + strSQLEnd
        ElseIf pLstOrg.Length = 0 AndAlso pLstPG.Length = 0 Then
            strSQL = strSQLStart + strSQLEnd
        End If

        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            strSQL = strSQL.Replace("%__tmp_epotpisi__%", pTmpTableName)
            strSQL = strSQL.Replace("%__ListOrgJed__%", pLstOrg)
            strSQL = strSQL.Replace("%__ListPGJed__%", pLstPG)
            strSQL = strSQL.Replace("%__Days__%", pDays)
            strSQL = strSQL.Replace("%__StatusText__%", pStatusTxt)
            strSQL = strSQL.Replace("%__EmployeeId__%", pEmployeeId)

            strSQL = IIf(pCreatedBy > 0, strSQL.Replace("%__CreatedBy__%", " AND u2e.employees_id = " + pCreatedBy.ToString),
                         strSQL.Replace("%__CreatedBy__%", ""))
            strSQL = IIf(pExtEmployees <> "", strSQL.Replace("%__ExtEmployees__%", " AND e.Title IN ('" + pExtEmployees + "')"), strSQL.Replace("%__ExtEmployees__%", ""))
            strSQL = IIf(pVrsta <> "", strSQL.Replace("%__Vrsta__%", " AND pn.vrsta_naloga IN (" + pVrsta + ")"), strSQL.Replace("%__Vrsta__%", ""))
            strSQL = IIf(pPnDoc <> -1, strSQL.Replace("%__pn_doc__%", "  AND pnp.pn_doc =  " + pPnDoc.ToString()), strSQL.Replace("%__pn_doc__%", "  AND pnp.pn_doc = -1"))

            mycmd.CommandText = strSQL

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)


            Me.PnPutniNalogList = JsonConvert.DeserializeObject(Of List(Of ngPnPutniNalog))(GetJson(DtList))

        End Using

    End Sub

    ' API L2
    ' byorg + bypg
    Public Sub getPnPGExtPutniNalog(ByVal pLstPGExt As String, ByVal pTmpTableName As String, ByVal pStatusTxt As String,
                                     ByVal pEmployeeId As Integer,
                                    Optional pDays As Integer = 30,
                                  Optional pCreatedBy As Integer = -1,
                                  Optional pExtEmployees As String = "",
                                   Optional pVrsta As String = "",
                                  Optional pPnDoc As Integer = -1)
        '
        ' Uzmi org.id iz sys_usr_role_json tabele za usera koji ima email: my_aspnet_membership.Email
        '
        'Dim _lstOrg As String = getOrgJed(pEmail)


        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL, strSQLStart, strSQLPG, strSQLEnd As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQLStart = <![CDATA[ 
DROP TABLE IF EXISTS %__tmp_epotpisi__%;

CREATE TEMPORARY TABLE %__tmp_epotpisi__% (
  id INT(11) NOT NULL DEFAULT '0',
  isSignee INT(11) NOT NULL DEFAULT '0',
  PotpisId TEXT CHARACTER SET cp1250 NOT NULL, 
  Potpis LONGTEXT CHARACTER SET cp1250 NOT NULL,
  status_date  DATETIME DEFAULT NULL,
  created_by  VARCHAR(100) CHARACTER SET cp1250 DEFAULT NULL,
  PRIMARY KEY (id)
) ENGINE=INNODB DEFAULT CHARSET=latin1;

    ]]>.Value



        strSQLPG = <![CDATA[ 
INSERT IGNORE INTO %__tmp_epotpisi__%
SELECT
 pn.id, 
 IFNULL(MAX(pnp.id), -1) AS isSignee,
 IFNULL(GROUP_CONCAT(e.`sifra` ORDER BY pnp.`id` ASC),'') AS PotpisId,  
 IFNULL(GROUP_CONCAT(CONCAT(e.`prezime`,' ',e.`ime`)),'') AS Potpis
 ,NULL, NULL
FROM
  putninalog pn 
  INNER JOIN putninalog_status pns ON pn.status_naloga=pns.id
  INNER JOIN putninalog_user pnu ON pn.id=pnu.pn_id
  INNER JOIN evd_saradnici s ON pn.employee_id = s.`sifra`
  LEFT JOIN `evd_employees` es ON s.`sifra`=es.`EmployeeID`  
  LEFT JOIN putninalog_potpisi pnp ON pn.id=pnp.pn_id  %__pn_doc__%
  LEFT JOIN evd_potpisi ep ON pnp.evd_potpisi_id=ep.id
  LEFT JOIN `evd_saradnici` e ON ep.employee_id = e.`sifra`
WHERE (es.`EmployeeID` IS NULL OR es.`Aktivan` = 0) 
AND ( %__ListPGJed__% )
AND  pn.dat_poc_putovanja > DATE_ADD(DATE(NOW()), INTERVAL -%__Days__% DAY)
GROUP BY pn.id;  
    ]]>.Value


        strSQLEnd = <![CDATA[ 
UPDATE   %__tmp_epotpisi__% pn 
  INNER JOIN putninalog_potpisi pnp ON pn.id=pnp.pn_id  %__pn_doc__%
  INNER JOIN evd_potpisi ep ON pnp.evd_potpisi_id=ep.id
  INNER JOIN evd_employees e ON ep.employee_id=e.EmployeeID
SET pn.isSignee = -999
WHERE e.EmployeeID=%__EmployeeId__%
AND pnp.id = pn.`isSignee`;

UPDATE   %__tmp_epotpisi__% pn 
SET pn.isSignee = 0
WHERE pn.isSignee > 0;

UPDATE   %__tmp_epotpisi__% pn 
SET pn.isSignee = 1
WHERE pn.isSignee = -999;

UPDATE   %__tmp_epotpisi__% pn 
SET pn.isSignee = 0
WHERE pn.isSignee < 0;

UPDATE    %__tmp_epotpisi__% t1, 
(
SELECT pno.pn_id AS pn_id
,MAX(pno.dummytimestamp) AS dt
FROM putninalog_odobravanje pno
LEFT JOIN putninalog pn ON pno.pn_id=pn.id
AND  pn.dat_poc_putovanja > DATE_ADD(DATE(NOW()), INTERVAL -%__Days__% DAY)
GROUP BY pno.pn_id
ORDER BY pno.pn_id ASC
) t2
SET t1.status_date = t2.dt
WHERE t1.id = t2.pn_id;

UPDATE  %__tmp_epotpisi__% pn 
INNER JOIN putninalog_user pnu ON pnu.pn_id=pn.id 
LEFT JOIN my_aspnet_users mu ON pnu.user_id=mu.id 
LEFT JOIN my_aspnet_users_to_employees u2e ON u2e.users_id+1=mu.id 
LEFT JOIN evd_employees e ON u2e.employees_id=e.EmployeeID 
SET pn.created_by = CONCAT(e.FirstName,' ',e.LastName); 

DROP TABLE IF EXISTS %__tmp_epotpisi__%_tem;  
  CREATE TEMPORARY TABLE %__tmp_epotpisi__%_tem
  SELECT 
pnt1.pn_id, 
IFNULL(pnt1.pn_temeljnica,'') AS temeljnica_1,
IFNULL(pnt2.pn_temeljnica,'') AS temeljnica_2
FROM putninalog_temeljnica pnt1
INNER JOIN putninalog_temeljnica pnt2 ON pnt1.pn_id=pnt2.pn_id,
%__tmp_epotpisi__% t
WHERE pnt1.pn_vrsta=1 
AND pnt2.pn_vrsta=2
AND t.id=pnt1.pn_id 
AND  t.id=pnt1.pn_id;

SELECT
 pn.id, 
 pn.employee_id, 
 pn.ime_prezime, 
 pn.radno_mjesto,
 pn.mjesto_start, 
 pn.mjesto_putovanja, 
 pn.razlog_putovanja,
 DATE_FORMAT(pn.dat_poc_putovanja,'%Y-%m-%d') AS dat_poc_putovanja,
 pn.trajanje_putovanja,
 pn.relacija,   
 pn.prev_text,  
 pn.prev_auto AS PrevAuto,
 pn.prev_vlauto AS PrevVlAuto,
 pn.prev_avion AS PrevAvion,
 pn.prev_autobus AS PrevAutobus,
 pn.prevoz_voz AS PrevVoz, 
 pn.troskovi_putovanja_knjizenje,
 IFNULL(pn.aktivnost,'') AS aktivnost,   
 IFNULL(CONCAT(so.Naziv,' (',so.Sifra,')'),'') AS nazivorgjed,
 pn.org_jed,
 IFNULL(pn.ukupnitroskovi_a,0) AS UkTroskoviA,
 IFNULL(pn.ukupnitroskovi_b,0) AS UkTroskoviB,
 IFNULL(pn.ukupnitroskovi_c,0) AS UkTroskoviC,
 IFNULL(pn.ukupnitroskovi_d,0) AS UkTroskoviD,
 IFNULL(pn.iznos_obracuna,0) AS UkIznosObracuna,
 IFNULL(pn.iznos_razlika,0) AS UkIznosRazlika,
 pn.iznos_dnevnice, 
 pn.proc_dnevnice, 
 pn.broj_dnevnica, 
  pn.akontacija_dnevnice, 
  pn.akontacija_nocenje,  
  pn.akontacija_ostalo, 
  pn.iznos_akontacije,
  pn.`ispl_akontacije`,
 pn.odobrena_akontacija,
 pn.vrsta_naloga,
 pns.status_text AS status_naloga,
 DATE_FORMAT(IFNULL(pot.status_date,'0001-01-01 00:00:00'),'%Y-%m-%dT%H:%i:%s') AS status_date, 
 pot.created_by, 
 CONVERT(IFNULL(pot.Potpis,'') USING cp1250) AS potpis_naloga,
 pot.isSignee,
IFNULL(pnt.temeljnica_1,'') AS temeljnica_1,
IFNULL(pnt.temeljnica_2,'') AS temeljnica_2,
    (
    CASE 
        WHEN pe.`pn_protokol` IS NULL THEN '...'
        ELSE CONCAT(pe.`pn_protokol`, '/',pe.`pn_redbr`, '-',pe.`pn_mjesec`, '/',pe.`pn_godina`)
    END) AS pn_protokol,
IFNULL(e.`Aktivan`, 0) AS uposlenik    
FROM putninalog pn 
INNER JOIN putninalog_status pns ON pn.status_naloga=pns.id
LEFT JOIN evd_employees e ON pn.employee_id=e.EmployeeID
INNER JOIN %__tmp_epotpisi__% AS pot ON pn.id=pot.id
LEFT JOIN sfr_organizacija so ON e.DepartmentUP=so.id
LEFT JOIN putninalog_user pnu ON pn.id=pnu.pn_id
LEFT JOIN my_aspnet_users_to_employees u2e ON pnu.user_id-1=u2e.users_id
LEFT JOIN  %__tmp_epotpisi__%_tem
pnt on pn.id=pnt.pn_id
LEFT JOIN `putninalog_evidencija` pe ON pn.`id` = pe.`pn_id`
WHERE pns.status_text IN(%__StatusText__%)
%__CreatedBy__%
%__ExtEmployees__%
%__Vrsta__%
ORDER BY pn.dat_poc_putovanja;

    ]]>.Value


        strSQL = <![CDATA[ 
    ]]>.Value



        strSQL = strSQLStart + strSQLPG + strSQLEnd


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            strSQL = strSQL.Replace("%__tmp_epotpisi__%", pTmpTableName)
            strSQL = strSQL.Replace("%__ListPGJed__%", pLstPGExt)
            strSQL = strSQL.Replace("%__Days__%", pDays)
            strSQL = strSQL.Replace("%__StatusText__%", pStatusTxt)
            strSQL = strSQL.Replace("%__EmployeeId__%", pEmployeeId)

            strSQL = IIf(pCreatedBy > 0, strSQL.Replace("%__CreatedBy__%", " AND u2e.employees_id = " + pCreatedBy.ToString),
                         strSQL.Replace("%__CreatedBy__%", ""))
            strSQL = IIf(pExtEmployees <> "", strSQL.Replace("%__ExtEmployees__%", " AND e.Title IN ('" + pExtEmployees + "')"), strSQL.Replace("%__ExtEmployees__%", ""))
            strSQL = IIf(pVrsta <> "", strSQL.Replace("%__Vrsta__%", " AND pn.vrsta_naloga IN (" + pVrsta + ")"), strSQL.Replace("%__Vrsta__%", ""))
            strSQL = IIf(pPnDoc <> -1, strSQL.Replace("%__pn_doc__%", "  AND pnp.pn_doc =  " + pPnDoc.ToString()), strSQL.Replace("%__pn_doc__%", "  AND pnp.pn_doc = -1"))

            mycmd.CommandText = strSQL

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)

        End Using


        Dim _pnPutniNalogList As List(Of ngPnPutniNalog) = JsonConvert.DeserializeObject(Of List(Of ngPnPutniNalog))(GetJson(DtList))
        For Each el In _pnPutniNalogList
            Me.PnPutniNalogList.Add(el)
        Next

        _pnPutniNalogList = Me.PnPutniNalogList.Distinct().ToList()
        Me.PnPutniNalogList = _pnPutniNalogList

    End Sub



    Private Function getOrgJed(ByVal pEmail As String) As String

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable
        Dim dbRead As MySqlDataReader

        Dim _lstOrg As String = ""
        Dim _lstOrgInt As New List(Of Integer)
        Dim _lstOrgSubInt As New List(Of Integer)

        '
        ' Lista red gdje je Form: APIputnal
        '
        strSQL = <![CDATA[ 
SELECT u2e.employees_id, urj.Name AS org, urj.NamePG AS pg
FROM my_aspnet_membership m
INNER JOIN my_aspnet_users_to_employees u2e ON m.userId=u2e.users_id
INNER JOIN sys_usr_role_json urj ON CONVERT(m.Email USING utf8)=CONVERT(urj.Type USING utf8)
WHERE urj.Form='APIputnal'
AND m.Email=@Email
    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)

            myconnection.Open()
            Dim mycmd As New MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@Email", pEmail)
            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader
            If dbRead.HasRows Then
                While dbRead.Read
                    _lstOrg = dbRead.GetString("org")
                End While
            End If
            dbRead.Close()

        End Using

        If _lstOrg.Length > 0 Then
            Dim _lstOrgS As String() = _lstOrg.Split(New Char() {","c})

            Dim iConv As Integer
            For Each part As String In _lstOrgS
                If Integer.TryParse(part, iConv) = True Then
                    _lstOrgInt.Add(iConv)
                End If
            Next
        End If

        '
        ' Ima org.jed. u listi
        '
        If _lstOrgInt.Count > 0 Then

            '_lstOrg = String.Join(",", _lstOrgInt.ToArray())

            strSQL = <![CDATA[ 
SELECT s.id, s.Sifra AS Sifra, s.`Sifra Nadnivo` AS SifraNadnivo 
FROM sfr_organizacija s;
    ]]>.Value


            Using myconnection As New MySqlConnection(ConnectionString)

                myconnection.Open()
                Dim mycmd As New MySqlCommand
                mycmd.Connection = myconnection

                mycmd.CommandText = strSQL


                DtList.Clear()
                myda = New MySqlDataAdapter(mycmd)
                myda.Fill(DtList)

            End Using
        End If

        For Each el As Integer In _lstOrgInt
            getOrgJed2(_lstOrgSubInt, el, DtList)
        Next

        _lstOrg = ""
        For Each el As Integer In _lstOrgInt
            _lstOrgSubInt.Add(el)
        Next

        If _lstOrgSubInt.Count > 0 Then
            _lstOrg = String.Join(",", _lstOrgSubInt.ToArray())
        End If

        Return _lstOrg

    End Function

    Private Sub getOrgJed2(ByRef pOrgList As List(Of Integer), ByVal pOrgId As Integer, ByRef pDataTbl As DataTable)

        Dim _OrgList As New List(Of Integer)

        Dim result() As DataRow = pDataTbl.Select("SifraNadnivo = '" + pOrgId.ToString + "'")
        Dim _resInt As Integer
        If result.Length > 0 Then
            For Each row As DataRow In result
                If Integer.TryParse(row("id"), _resInt) Then
                    pOrgList.Add(_resInt)
                    _OrgList.Add(_resInt)
                End If
            Next

            For Each el As Integer In _OrgList
                getOrgJed2(pOrgList, el, pDataTbl)
            Next
        Else
            Exit Sub
        End If

    End Sub

    ' API L1
    ' bypg
    Public Sub getPnPgPutniNalog(ByVal pEmail As String)

        '
        ' Uzmi org.id iz sys_usr_role_json tabele za usera koji ima email: my_aspnet_membership.Email
        '
        Dim _lstPg As String = getPg(pEmail)


        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable


        strSQL = <![CDATA[ 
SELECT
 pn.`id`, 
 pn.`employee_id`, 
 pn.`ime_prezime`, 
 pn.radno_mjesto,
 pn.`mjesto_start`, 
 pn.`mjesto_putovanja`, 
 pn.`razlog_putovanja`,
 DATE_FORMAT(pn.`dat_poc_putovanja`,'%Y-%m-%d') AS dat_poc_putovanja,
 pn.`trajanje_putovanja`,
 pn.relacija,   
 pn.prev_text,  
 pn.prev_auto AS PrevAuto,
 pn.prev_vlauto AS PrevVlAuto,
 pn.prev_avion AS PrevAvion,
 pn.prev_autobus AS PrevAutobus,
 pn.prevoz_voz AS PrevVoz, 
 pn.`troskovi_putovanja_knjizenje`,
 IFNULL(pn.aktivnost,'') AS aktivnost,   
 CONCAT(so.Naziv,' (',so.`Sifra`,')') AS nazivorgjed,
 pn.`org_jed`,
 IFNULL(pn.ukupnitroskovi_a,0) AS UkTroskoviA,
 IFNULL(pn.ukupnitroskovi_b,0) AS UkTroskoviB,
 IFNULL(pn.ukupnitroskovi_c,0) AS UkTroskoviC,
 IFNULL(pn.ukupnitroskovi_d,0) AS UkTroskoviD,
 IFNULL(pn.iznos_obracuna,0) AS UkIznosObracuna,
 IFNULL(pn.iznos_razlika,0) AS UkIznosRazlika,
 pn.iznos_dnevnice, 
 pn.proc_dnevnice, 
 pn.broj_dnevnica, 
  pn.akontacija_dnevnice, 
  pn.akontacija_nocenje,  
  pn.akontacija_ostalo, 
  pn.`iznos_akontacije`,
  pn.`ispl_akontacije`,
 pn.odobrena_akontacija,
 pn.vrsta_naloga,
 pns.`status_text` AS `status_naloga`,
 CONVERT(IFNULL(pot.Potpis,'') USING cp1250) AS potpis_naloga
FROM
  putninalog pn INNER JOIN`putninalog_status` pns ON pn.`status_naloga`=pns.`id`
INNER JOIN `evd_employees` e ON pn.`employee_id`=e.`EmployeeID`
  LEFT JOIN `putninalog_potpisi` pnp ON pn.`id`=pnp.`pn_id`
  LEFT JOIN `evd_potpisi` ep ON pnp.`evd_potpisi_id`=ep.`id`
  LEFT JOIN (SELECT
 pn.`id`, 
 GROUP_CONCAT(CONCAT(e.`LastName`,' ',e.`FirstName`)) AS Potpis
FROM
  putninalog pn 
  INNER JOIN`putninalog_status` pns ON pn.`status_naloga`=pns.`id`
  INNER JOIN `putninalog_user` pnu ON pn.`id`=pnu.`pn_id`
  INNER JOIN `evd_employees` e1 ON pn.`employee_id`=e1.`EmployeeID`
  LEFT JOIN `putninalog_potpisi` pnp ON pn.`id`=pnp.`pn_id`
  LEFT JOIN `evd_potpisi` ep ON pnp.`evd_potpisi_id`=ep.`id`
  LEFT JOIN `evd_employees` e ON ep.`employee_id`=e.`EmployeeID`
WHERE e1.`EmployeeID` IN(%__ListOrgJed__%)
GROUP BY pn.`id`) AS pot ON pn.`id`=pot.id
LEFT JOIN sfr_organizacija so ON e.DepartmentUP=so.id
WHERE e.EmployeeID IN(%__ListOrgJed__%);
    ]]>.Value




        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            strSQL = strSQL.Replace("%__ListOrgJed__%", _lstPg)
            mycmd.CommandText = strSQL

            If _lstPg.Length > 0 Then
                DtList.Clear()
                myda = New MySqlDataAdapter(mycmd)
                myda.Fill(DtList)
            End If

            Me.PnPutniNalogList = JsonConvert.DeserializeObject(Of List(Of ngPnPutniNalog))(GetJson(DtList))

        End Using

    End Sub

    Private Function getPg(ByVal pEmail As String) As String

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable
        Dim dbRead As MySqlDataReader

        Dim _lstPg As String = ""
        Dim _lstOrgInt As New List(Of Integer)
        Dim _lstOrgSubInt As New List(Of Integer)
        Dim _lstEmpIds As String = ""

        '
        ' Lista red gdje je Form: APIputnal
        '
        strSQL = <![CDATA[ 
SELECT u2e.employees_id, urj.Name AS org, urj.NamePG AS pg
FROM my_aspnet_membership m
INNER JOIN my_aspnet_users_to_employees u2e ON m.userId=u2e.users_id
INNER JOIN sys_usr_role_json urj ON CONVERT(m.Email USING utf8)=CONVERT(urj.Type USING utf8)
WHERE urj.Form='APIputnal'
AND m.Email=@Email
    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)

            myconnection.Open()
            Dim mycmd As New MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@Email", pEmail)
            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader
            If dbRead.HasRows Then
                While dbRead.Read
                    _lstPg = dbRead.GetString("pg")
                End While
            End If
            dbRead.Close()

        End Using

        If _lstPg.Length > 0 Then
            Dim _lstOrgS As String() = _lstPg.Split(New Char() {","c})

            Dim iConv As Integer
            For Each part As String In _lstOrgS
                If Integer.TryParse(part, iConv) = True Then
                    _lstOrgInt.Add(iConv)
                End If
            Next
        Else
            Return _lstEmpIds
        End If

        '
        ' Ima org.jed. u listi
        '
        If _lstOrgInt.Count > 0 Then

            '_lstOrg = String.Join(",", _lstOrgInt.ToArray())

            strSQL = <![CDATA[ 
SELECT s.id, s.Sifra AS Sifra, s.`Sifra Nadnivo` AS SifraNadnivo 
FROM sfr_poslovnegrupe s;
    ]]>.Value


            Using myconnection As New MySqlConnection(ConnectionString)

                myconnection.Open()
                Dim mycmd As New MySqlCommand
                mycmd.Connection = myconnection

                mycmd.CommandText = strSQL

                DtList.Clear()
                myda = New MySqlDataAdapter(mycmd)
                myda.Fill(DtList)

            End Using
        End If

        For Each el As Integer In _lstOrgInt
            getOrgJed2(_lstOrgSubInt, el, DtList)
        Next

        _lstPg = ""
        For Each el As Integer In _lstOrgInt
            _lstOrgSubInt.Add(el)
        Next

        If _lstOrgSubInt.Count > 0 Then
            For Each el In _lstOrgSubInt
                _lstPg += "'" + el.ToString + "'" + ","
            Next
            '_lstPg = String.Join(",", _lstOrgSubInt.ToArray())
        End If
        _lstPg = _lstPg.TrimEnd(New Char() {","c})

        strSQL = <![CDATA[ 
SELECT s.Opis AS EmpIds
FROM sfr_poslovnegrupe s
WHERE s.Sifra IN(%__ListOrgJed__%);
    ]]>.Value
        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            strSQL = strSQL.Replace("%__ListOrgJed__%", _lstPg)
            mycmd.CommandText = strSQL

            dbRead = mycmd.ExecuteReader
            If dbRead.HasRows Then
                While dbRead.Read
                    _lstEmpIds += dbRead.GetString("EmpIds") + ","
                End While
            End If


        End Using
        _lstEmpIds = _lstEmpIds.TrimEnd(New Char() {","c})

        Return _lstEmpIds

    End Function

    ' API L5
    Public Function getPnPutniNalogById(ByVal pId As Integer, ByVal pEmpId As Integer,
                                        Optional pVrsta As String = "") As ngPnPutniNalog

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT
 pn.`id`, 
 pn.`employee_id`, 
 pn.`ime_prezime`, 
 pn.radno_mjesto,
 pn.`mjesto_start`, 
 pn.`mjesto_putovanja`, 
 pn.`razlog_putovanja`,
 DATE_FORMAT(pn.`dat_poc_putovanja`,'%Y-%m-%d') AS dat_poc_putovanja,
 pn.`trajanje_putovanja`,
 pn.relacija,  
 pn.prev_text,   
 pn.prev_auto AS PrevAuto,
 pn.prev_vlauto AS PrevVlAuto,
 pn.prev_avion AS PrevAvion,
 pn.prev_autobus AS PrevAutobus,
 pn.prevoz_voz AS PrevVoz, 
 pn.`troskovi_putovanja_knjizenje`,
 IFNULL(pn.aktivnost,'') AS aktivnost,   
 pn.`org_jed`,
 IFNULL(pn.ukupnitroskovi_a,0) AS UkTroskoviA,
 IFNULL(pn.ukupnitroskovi_b,0) AS UkTroskoviB,
 IFNULL(pn.ukupnitroskovi_c,0) AS UkTroskoviC,
 IFNULL(pn.ukupnitroskovi_d,0) AS UkTroskoviD,
 IFNULL(pn.iznos_obracuna,0) AS UkIznosObracuna,
 IFNULL(pn.iznos_razlika,0) AS UkIznosRazlika,
  pn.iznos_dnevnice, 
  pn.proc_dnevnice, 
 pn.broj_dnevnica, 
  pn.akontacija_dnevnice, 
  pn.akontacija_nocenje,  
  pn.akontacija_ostalo, 
  pn.`iznos_akontacije`,
  pn.`ispl_akontacije`,
 pn.odobrena_akontacija,
 pn.vrsta_naloga,  
 pns.`status_text` AS `status_naloga`,
 IF(e.`EmployeeID`= @pEmployeeId, 1, 0) AS isSignee,
 CONVERT(IFNULL(GROUP_CONCAT(CONCAT(e.`LastName`,' ',e.`FirstName`) ORDER BY pnp.`id` ASC),'') USING cp1250) AS potpis_naloga,
 IFNULL(ee.`Aktivan`, 0) AS uposlenik   
FROM
  putninalog pn 
  INNER JOIN`putninalog_status` pns ON pn.`status_naloga`=pns.`id`
  INNER JOIN `putninalog_user` pnu ON pn.`id`=pnu.`pn_id`
  LEFT JOIN `putninalog_potpisi` pnp ON pn.`id`=pnp.`pn_id`
  LEFT JOIN `evd_potpisi` ep ON pnp.`evd_potpisi_id`=ep.`id`
  LEFT JOIN `evd_employees` e ON ep.`employee_id`=e.`EmployeeID`  
  LEFT JOIN `evd_employees` ee ON pn.`employee_id`=ee.`EmployeeID`  
WHERE pn.`id`=@Id %__Vrsta__%;
    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection
            strSQL = IIf(pVrsta <> "", strSQL.Replace("%__Vrsta__%", " AND pn.vrsta_naloga IN (" + pVrsta + ")"), strSQL.Replace("%__Vrsta__%", ""))

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@Id", pId)
            mycmd.Parameters.AddWithValue("@pEmployeeId", pEmpId)
            mycmd.Prepare()

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            'myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(DtList)

            'DtList.Rows.Add("post", "White", "Ukupno sati", "UKP", 0)

            Me.PnPutniNalogList = JsonConvert.DeserializeObject(Of List(Of ngPnPutniNalog))(GetJson(DtList))

            If Me.PnPutniNalogList.Count > 0 Then
                Return PnPutniNalogList.Item(0)
            Else
                Return Nothing
            End If


        End Using

    End Function

    ' API L6
    Public Function getPnPutniNalogIzvById(ByVal pId As Integer) As ngPnPutniNalog

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT
 pn.`id`, 
 pn.`employee_id`, 
 pn.`ime_prezime`, 
 pn.radno_mjesto,
 pn.`mjesto_start`, 
 pn.`mjesto_putovanja`, 
 pn.`razlog_putovanja`,
 DATE_FORMAT(pn.`dat_poc_putovanja`,'%Y-%m-%d') AS dat_poc_putovanja,
 pn.`trajanje_putovanja`,
 pn.relacija,
  pn.prev_text,    
 pn.prev_auto AS PrevAuto,
 pn.prev_vlauto AS PrevVlAuto,
 pn.prev_avion AS PrevAvion,
 pn.prev_autobus AS PrevAutobus,
 pn.prevoz_voz AS PrevVoz, 
 pn.`troskovi_putovanja_knjizenje`,
 IFNULL(pn.aktivnost,'') AS aktivnost,   
 pn.`org_jed`,
 IFNULL(pn.ukupnitroskovi_a,0) AS UkTroskoviA,
 IFNULL(pn.ukupnitroskovi_b,0) AS UkTroskoviB,
 IFNULL(pn.ukupnitroskovi_c,0) AS UkTroskoviC,
 IFNULL(pn.ukupnitroskovi_d,0) AS UkTroskoviD,
 IFNULL(pn.iznos_obracuna,0) AS UkIznosObracuna,
 IFNULL(pn.iznos_razlika,0) AS UkIznosRazlika,
  pn.iznos_dnevnice, 
  pn.proc_dnevnice, 
 pn.broj_dnevnica,
  pn.akontacija_dnevnice, 
  pn.akontacija_nocenje,  
  pn.akontacija_ostalo, 
  pn.`iznos_akontacije`,
  pn.`ispl_akontacije`,
 pn.odobrena_akontacija,
 pn.vrsta_naloga,  
 pns.`status_text` AS `status_naloga`,
 pni.pn_id AS id, 
 IFNULL(pni.pn_izvjestaj,'') AS izvjestaj  
FROM
  putninalog pn INNER JOIN`putninalog_status` pns ON pn.`status_naloga`=pns.`id`
  LEFT JOIN `putninalog_izvjestaj` pni ON pn.`id`=pni.`pn_id`
WHERE pn.id=@Id;
    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            'Dim dbRead As MySqlDataReader
            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@Id", pId)
            mycmd.Prepare()

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            'myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(DtList)

            'DtList.Rows.Add("post", "White", "Ukupno sati", "UKP", 0)

            Me.PnPutniNalogList = JsonConvert.DeserializeObject(Of List(Of ngPnPutniNalog))(GetJson(DtList))
            Return PnPutniNalogList.Item(0)

        End Using

    End Function

    ' API esC2
    Public Function getPnPotpis_1_ById(ByVal pId As Integer,
                                        Optional pVrsta As String = "") As ngPnPutniNalog

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT
 pn.`id`, 
 pn.`employee_id`, 
 pn.`ime_prezime`, 
 pn.radno_mjesto,
 pn.`mjesto_start`, 
 pn.`mjesto_putovanja`, 
 pn.`razlog_putovanja`,
 DATE_FORMAT(pn.`dat_poc_putovanja`,'%Y-%m-%d') AS dat_poc_putovanja,
 pn.`trajanje_putovanja`,
 pn.relacija,  
 pn.prev_text,   
 pn.`troskovi_putovanja_knjizenje`,
 IFNULL(pn.aktivnost,'') AS aktivnost,   
 pn.`org_jed`,
  pn.iznos_dnevnice, 
  pn.proc_dnevnice, 
 pn.broj_dnevnica, 
  pn.akontacija_dnevnice, 
  pn.akontacija_nocenje,  
  pn.akontacija_ostalo, 
  pn.`iznos_akontacije`,
  pn.`ispl_akontacije`,
 pn.odobrena_akontacija,
 pn.vrsta_naloga,  
 pns.`status_text` AS `status_naloga`
FROM
  putninalog pn INNER JOIN`putninalog_status` pns ON pn.`status_naloga`=pns.`id`
WHERE pn.`id`=@Id %__Vrsta__%;
    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection
            strSQL = IIf(pVrsta <> "", strSQL.Replace("%__Vrsta__%", " AND pn.vrsta_naloga IN (" + pVrsta + ")"), strSQL.Replace("%__Vrsta__%", ""))

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@Id", pId)
            mycmd.Prepare()

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            'myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(DtList)

            'DtList.Rows.Add("post", "White", "Ukupno sati", "UKP", 0)

            Me.PnPutniNalogList = JsonConvert.DeserializeObject(Of List(Of ngPnPutniNalog))(GetJson(DtList))

            If Me.PnPutniNalogList.Count > 0 Then
                Return PnPutniNalogList.Item(0)
            Else
                Return Nothing
            End If


        End Using

    End Function

    ' API L7
    Public Function getPnPutniNalogStaById(ByVal pGetStatus As String, ByVal pFromStatus As String, ByVal pPnId As Integer) As String

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
    ]]>.Value



        Dim _retVal As String = ""

        Select Case pGetStatus
            Case "prev"
                strSQL = <![CDATA[ 
SELECT 'from' AS naziv, pnw2.`status_from` AS _status
FROM `putninalog_workflow` pnw2
WHERE pnw2.`status_to`=@status;
    ]]>.Value
            Case "next"
                strSQL = <![CDATA[ 
SELECT 'to' AS naziv, pnw1.`status_to` AS _status
FROM `putninalog_workflow` pnw1
WHERE pnw1.`status_from`=@status;
    ]]>.Value
            Case "current"
                'Return _retVal
                strSQL = <![CDATA[ 
SELECT 'current' AS naziv , pns.status_text  AS _status
FROM putninalog pn, putninalog_status pns
WHERE pn.status_naloga = pns.id
AND pn.id=@pnid;
    ]]>.Value
            Case Else
                Return _retVal

        End Select

        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlDataReader
            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()

            If pGetStatus = "current" Then
                mycmd.Parameters.AddWithValue("@pnid", pPnId)
            Else
                mycmd.Parameters.AddWithValue("@status", pFromStatus)
            End If

            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader
            If dbRead.HasRows Then
                While dbRead.Read()
                    _retVal = dbRead.GetString("_status")
                End While
            End If

        End Using

        Return _retVal

    End Function

    ' API L4
    Public Function getPnPutniNalogDetById(ByVal pId As Integer) As NgPnPutniNalogDet

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT
  id, 
  pn_id, 
  pn_oblast, 
  IFNULL(relacija,'') AS relacija, 
 DATE_FORMAT(TIMESTAMP(IFNULL(dat_polaska,'0001-01-01'),IFNULL(vri_polaska,'00:00')),'%Y-%m-%dT%H:%i:%s') AS dat_polaska,   
  DATE_FORMAT(TIMESTAMP(IFNULL(dat_povratka,'0001-01-01'),IFNULL(vri_povratka,'00:00')),'%Y-%m-%dT%H:%i:%s') AS dat_povratka, 
  IFNULL(broj_sati,0) AS broj_sati, 
  IFNULL(broj_dnevnica, 0) AS broj_dnevnica, 
  IFNULL(iznos_dnevnice,0) AS iznos_dnevnice, 
  IFNULL(ukupan_iznos,0) AS ukupan_iznos, 
  IFNULL(vrsta_prevoza,'') AS vrsta_prevoza, 
  IFNULL(razred,'') AS razred, 
  IFNULL(iznos_karte,0) AS iznos_karte, 
  IFNULL(naknada_km,0) AS naknada_km, 
  IFNULL(troskovi_goriva,0) AS troskovi_goriva, 
  IFNULL(troskovi_parkinga,0) AS troskovi_parkinga, 
  IFNULL(ostali_troskovi,0) AS ostali_troskovi, 
  IFNULL(obracun_ostalih_troskova,'') AS obr_ostalih_troskova, 
  order_id
FROM
putninalog_obracun
WHERE pn_id=@Id
ORDER BY pn_oblast, order_id;
    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            'Dim dbRead As MySqlDataReader
            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@Id", pId)
            mycmd.Prepare()

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            'myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(DtList)

            'DtList.Rows.Add("post", "White", "Ukupno sati", "UKP", 0)

            Me.PnPutniNalogDet = JsonConvert.DeserializeObject(Of List(Of NgPnPutniNalogDet))(GetJson(DtList))
            Return PnPutniNalogDet.Item(0)

        End Using

    End Function

    ' API L8i
    Public Function getPnPutniNalogIzvDetById(ByVal pId As Integer) As NgPnPutniNalogDet

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 
SELECT
  id, 
  pn_id, 
  pn_oblast, 
  IFNULL(relacija,'') AS relacija, 
 DATE_FORMAT(TIMESTAMP(IFNULL(dat_polaska,'0001-01-01'),IFNULL(vri_polaska,'00:00')),'%Y-%m-%dT%H:%i:%s') AS dat_polaska,   
  DATE_FORMAT(TIMESTAMP(IFNULL(dat_povratka,'0001-01-01'),IFNULL(vri_povratka,'00:00')),'%Y-%m-%dT%H:%i:%s') AS dat_povratka, 
  IFNULL(broj_sati,0) AS broj_sati, 
  IFNULL(broj_dnevnica, 0) AS broj_dnevnica, 
  IFNULL(iznos_dnevnice,0) AS iznos_dnevnice, 
  IFNULL(ukupan_iznos,0) AS ukupan_iznos, 
  IFNULL(vrsta_prevoza,'') AS vrsta_prevoza, 
  IFNULL(razred,'') AS razred, 
  IFNULL(iznos_karte,0) AS iznos_karte, 
  IFNULL(naknada_km,0) AS naknada_km, 
  IFNULL(troskovi_goriva,0) AS troskovi_goriva, 
  IFNULL(troskovi_parkinga,0) AS troskovi_parkinga, 
  IFNULL(ostali_troskovi,0) AS ostali_troskovi, 
  IFNULL(obracun_ostalih_troskova,'') AS obr_ostalih_troskova, 
  order_id
FROM
putninalog_izvjestaj_dod
WHERE pn_id=@Id
ORDER BY pn_oblast, order_id;
    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            'Dim dbRead As MySqlDataReader
            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@Id", pId)
            mycmd.Prepare()

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            'myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(DtList)

            'DtList.Rows.Add("post", "White", "Ukupno sati", "UKP", 0)

            If DtList.Rows.Count > 0 Then
                Me.PnPutniNalogDet = JsonConvert.DeserializeObject(Of List(Of NgPnPutniNalogDet))(GetJson(DtList))
                Return PnPutniNalogDet.Item(0)
            End If


        End Using

        Return Nothing

    End Function


    ' API L9i
    ' byorg
    Public Sub getPnOrgPutniNalogIzvDet(ByVal pLstOrg As String, ByVal pTmpTableName As String, ByVal pStatusTxt As String,
                                  ByVal pEmployeeId As Integer,
                                  Optional pDays As Integer = 30,
                                  Optional pCreatedBy As Integer = -1)

        '
        ' Uzmi org.id iz sys_usr_role_json tabele za usera koji ima email: my_aspnet_membership.Email
        '
        'Dim _lstOrg As String = getOrgJed(pEmail)


        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable


        strSQL = <![CDATA[ 
DROP TABLE IF EXISTS %__tmp_epotpisi__%;

CREATE TEMPORARY TABLE `%__tmp_epotpisi__%` (
  `id` INT(11) NOT NULL DEFAULT '0',
  `isSignee` INT(11) NOT NULL DEFAULT '0',
  `Potpis` LONGTEXT CHARACTER SET cp1250 NOT NULL,
  status_date  DATETIME DEFAULT NULL,
  created_by  VARCHAR(100) CHARACTER SET cp1250 DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=INNODB DEFAULT CHARSET=latin1;

INSERT IGNORE INTO %__tmp_epotpisi__%
SELECT
 pn.`id`, 
 0 AS isSignee,
 IFNULL(GROUP_CONCAT(CONCAT(e.`LastName`,' ',e.`FirstName`)),'') AS Potpis
 ,NULL, NULL
FROM
  putninalog pn 
  INNER JOIN`putninalog_status` pns ON pn.`status_naloga`=pns.`id`
  INNER JOIN `putninalog_user` pnu ON pn.`id`=pnu.`pn_id`
  INNER JOIN `evd_employees` e1 ON pn.`employee_id`=e1.`EmployeeID`
  LEFT JOIN `putninalog_potpisi` pnp ON pn.`id`=pnp.`pn_id`
  LEFT JOIN `evd_potpisi` ep ON pnp.`evd_potpisi_id`=ep.`id`
  LEFT JOIN `evd_employees` e ON ep.`employee_id`=e.`EmployeeID`
WHERE e1.`DepartmentUP` IN(SELECT so.`id` 
FROM `sfr_organizacija` so
WHERE so.`Sifra` IN(%__ListOrgJed__%))
AND  pn.`dat_poc_putovanja` > DATE_ADD(DATE(NOW()), INTERVAL -%__Days__% DAY)
GROUP BY pn.`id`;

UPDATE   %__tmp_epotpisi__% pn 
  INNER JOIN `putninalog_potpisi` pnp ON pn.`id`=pnp.`pn_id`
  INNER JOIN `evd_potpisi` ep ON pnp.`evd_potpisi_id`=ep.`id`
  INNER JOIN `evd_employees` e ON ep.`employee_id`=e.`EmployeeID`
SET pn.isSignee = 1
WHERE e.`EmployeeID`=%__EmployeeId__%;

UPDATE   %__tmp_epotpisi__% pn 
  INNER JOIN `putninalog_potpisi` pnp ON pn.`id`=pnp.`pn_id`
  INNER JOIN `evd_potpisi` ep ON pnp.`evd_potpisi_id`=ep.`id`
  INNER JOIN `evd_potpisi_status` eps ON ep.`id`=eps.`id`
  INNER JOIN `evd_employees` e ON ep.`employee_id`=e.`EmployeeID`
SET pn.isSignee = 2
WHERE e.`EmployeeID`=%__EmployeeId__%;


UPDATE    %__tmp_epotpisi__% t1, 
(
SELECT pno.pn_id AS pn_id
,MAX(pno.dummytimestamp) AS dt
FROM putninalog_odobravanje pno
LEFT JOIN putninalog pn ON pno.pn_id=pn.id
AND  pn.dat_poc_putovanja > DATE_ADD(DATE(NOW()), INTERVAL -%__Days__% DAY)
GROUP BY pno.pn_id
ORDER BY pno.pn_id ASC
) t2
SET t1.status_date = t2.dt
WHERE t1.id = t2.pn_id;

UPDATE  %__tmp_epotpisi__% pn 
INNER JOIN putninalog_user pnu ON pnu.pn_id=pn.id 
LEFT JOIN my_aspnet_users mu ON pnu.user_id=mu.id 
LEFT JOIN my_aspnet_users_to_employees u2e ON u2e.users_id+1=mu.id 
LEFT JOIN evd_employees e ON u2e.employees_id=e.EmployeeID 
SET pn.created_by = CONCAT(e.FirstName,' ',e.LastName); 


SELECT
  pnd.id, 
  pnd.pn_id, 
  pnd.pn_oblast, 
  IFNULL(pnd.relacija,'') AS relacija, 
 DATE_FORMAT(TIMESTAMP(IFNULL(pnd.dat_polaska,'0001-01-01'),IFNULL(pnd.vri_polaska,'00:00')),'%Y-%m-%dT%H:%i:%s') AS dat_polaska,   
  DATE_FORMAT(TIMESTAMP(IFNULL(pnd.dat_povratka,'0001-01-01'),IFNULL(pnd.vri_povratka,'00:00')),'%Y-%m-%dT%H:%i:%s') AS dat_povratka, 
  IFNULL(pnd.broj_sati,0) AS broj_sati, 
  IFNULL(pnd.broj_dnevnica, 0) AS broj_dnevnica, 
  IFNULL(pnd.iznos_dnevnice,0) AS iznos_dnevnice, 
  IFNULL(pnd.ukupan_iznos,0) AS ukupan_iznos, 
  IFNULL(pnd.vrsta_prevoza,'') AS vrsta_prevoza, 
  IFNULL(pnd.razred,'') AS razred, 
  IFNULL(pnd.iznos_karte,0) AS iznos_karte, 
  IFNULL(pnd.naknada_km,0) AS naknada_km, 
  IFNULL(pnd.troskovi_goriva,0) AS troskovi_goriva, 
  IFNULL(pnd.troskovi_parkinga,0) AS troskovi_parkinga, 
  IFNULL(pnd.ostali_troskovi,0) AS ostali_troskovi, 
  IFNULL(pnd.obracun_ostalih_troskova,'') AS obr_ostalih_troskova, 
  pnd.order_id
FROM
putninalog_izvjestaj_dod pnd
INNER JOIN putninalog pn ON pnd.pn_id=pn.id
INNER JOIN putninalog_status pns ON pn.status_naloga=pns.id
INNER JOIN evd_employees e ON pn.employee_id=e.EmployeeID
INNER JOIN %__tmp_epotpisi__% AS pot ON pn.id=pot.id
LEFT JOIN sfr_organizacija so ON e.DepartmentUP=so.id
LEFT JOIN putninalog_user pnu ON pn.id=pnu.pn_id
LEFT JOIN my_aspnet_users_to_employees u2e ON pnu.user_id-1=u2e.users_id
WHERE pns.status_text IN(%__StatusText__%)
%__CreatedBy__%
ORDER BY pnd.pn_id, pnd.pn_oblast, pnd.order_id;

    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            strSQL = strSQL.Replace("%__tmp_epotpisi__%", pTmpTableName)
            strSQL = strSQL.Replace("%__ListOrgJed__%", pLstOrg)
            strSQL = strSQL.Replace("%__Days__%", pDays)
            strSQL = strSQL.Replace("%__StatusText__%", pStatusTxt)
            strSQL = strSQL.Replace("%__EmployeeId__%", pEmployeeId)

            strSQL = IIf(pCreatedBy > 0, strSQL.Replace("%__CreatedBy__%", " AND u2e.employees_id = " + pCreatedBy.ToString),
                         strSQL.Replace("%__CreatedBy__%", ""))
            mycmd.CommandText = strSQL

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)


            Me.PnPutniNalogIzvDetList = JsonConvert.DeserializeObject(Of List(Of NgPnPutniNalogDet))(GetJson(DtList))

        End Using

    End Sub

    ' API L9i
    ' bypg
    Public Sub getPnPGPutniNalogIzvDet(ByVal pLstOrg As String, ByVal pTmpTableName As String, ByVal pStatusTxt As String,
                                 ByVal pEmployeeId As Integer,
                                 Optional pDays As Integer = 30,
                                  Optional pCreatedBy As Integer = -1)

        '
        ' Uzmi org.id iz sys_usr_role_json tabele za usera koji ima email: my_aspnet_membership.Email
        '
        'Dim _lstOrg As String = getOrgJed(pEmail)


        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable



        strSQL = <![CDATA[ 
DROP TABLE IF EXISTS %__tmp_epotpisi__%;

CREATE TEMPORARY TABLE `%__tmp_epotpisi__%` (
  `id` INT(11) NOT NULL DEFAULT '0',
  `isSignee` INT(11) NOT NULL DEFAULT '0',
  `Potpis` LONGTEXT CHARACTER SET cp1250 NOT NULL,
  status_date  DATETIME DEFAULT NULL,
  created_by  VARCHAR(100) CHARACTER SET cp1250 DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=INNODB DEFAULT CHARSET=latin1;

INSERT IGNORE INTO %__tmp_epotpisi__%
SELECT
 pn.`id`, 
 0 AS isSignee,
 IFNULL(GROUP_CONCAT(CONCAT(e.`LastName`,' ',e.`FirstName`)),'') AS Potpis
 ,NULL, NULL
FROM
  putninalog pn 
  INNER JOIN`putninalog_status` pns ON pn.`status_naloga`=pns.`id`
  INNER JOIN `putninalog_user` pnu ON pn.`id`=pnu.`pn_id`
  INNER JOIN `evd_employees` e1 ON pn.`employee_id`=e1.`EmployeeID`
  LEFT JOIN `putninalog_potpisi` pnp ON pn.`id`=pnp.`pn_id`
  LEFT JOIN `evd_potpisi` ep ON pnp.`evd_potpisi_id`=ep.`id`
  LEFT JOIN `evd_employees` e ON ep.`employee_id`=e.`EmployeeID`
WHERE  e1.`EmployeeID`  IN(%__ListOrgJed__%)
AND  pn.`dat_poc_putovanja` > DATE_ADD(DATE(NOW()), INTERVAL -%__Days__% DAY)
GROUP BY pn.`id`;

UPDATE   %__tmp_epotpisi__% pn 
  INNER JOIN `putninalog_potpisi` pnp ON pn.`id`=pnp.`pn_id`
  INNER JOIN `evd_potpisi` ep ON pnp.`evd_potpisi_id`=ep.`id`
  INNER JOIN `evd_employees` e ON ep.`employee_id`=e.`EmployeeID`
SET pn.isSignee = 1
WHERE e.`EmployeeID`=%__EmployeeId__%;

UPDATE   %__tmp_epotpisi__% pn 
  INNER JOIN `putninalog_potpisi` pnp ON pn.`id`=pnp.`pn_id`
  INNER JOIN `evd_potpisi` ep ON pnp.`evd_potpisi_id`=ep.`id`
  INNER JOIN `evd_potpisi_status` eps ON ep.`id`=eps.`id`
  INNER JOIN `evd_employees` e ON ep.`employee_id`=e.`EmployeeID`
SET pn.isSignee = 2
WHERE e.`EmployeeID`=%__EmployeeId__%;


UPDATE    %__tmp_epotpisi__% t1, 
(
SELECT pno.pn_id AS pn_id
,MAX(pno.dummytimestamp) AS dt
FROM putninalog_odobravanje pno
LEFT JOIN putninalog pn ON pno.pn_id=pn.id
AND  pn.dat_poc_putovanja > DATE_ADD(DATE(NOW()), INTERVAL -%__Days__% DAY)
GROUP BY pno.pn_id
ORDER BY pno.pn_id ASC
) t2
SET t1.status_date = t2.dt
WHERE t1.id = t2.pn_id;

UPDATE  %__tmp_epotpisi__% pn 
INNER JOIN putninalog_user pnu ON pnu.pn_id=pn.id 
LEFT JOIN my_aspnet_users mu ON pnu.user_id=mu.id 
LEFT JOIN my_aspnet_users_to_employees u2e ON u2e.users_id+1=mu.id 
LEFT JOIN evd_employees e ON u2e.employees_id=e.EmployeeID 
SET pn.created_by = CONCAT(e.FirstName,' ',e.LastName); 



SELECT
  pnd.id, 
  pnd.pn_id, 
  pnd.pn_oblast, 
  IFNULL(pnd.relacija,'') AS relacija, 
 DATE_FORMAT(TIMESTAMP(IFNULL(pnd.dat_polaska,'0001-01-01'),IFNULL(pnd.vri_polaska,'00:00')),'%Y-%m-%dT%H:%i:%s') AS dat_polaska,   
  DATE_FORMAT(TIMESTAMP(IFNULL(pnd.dat_povratka,'0001-01-01'),IFNULL(pnd.vri_povratka,'00:00')),'%Y-%m-%dT%H:%i:%s') AS dat_povratka, 
  IFNULL(pnd.broj_sati,0) AS broj_sati, 
  IFNULL(pnd.broj_dnevnica, 0) AS broj_dnevnica, 
  IFNULL(pnd.iznos_dnevnice,0) AS iznos_dnevnice, 
  IFNULL(pnd.ukupan_iznos,0) AS ukupan_iznos, 
  IFNULL(pnd.vrsta_prevoza,'') AS vrsta_prevoza, 
  IFNULL(pnd.razred,'') AS razred, 
  IFNULL(pnd.iznos_karte,0) AS iznos_karte, 
  IFNULL(pnd.naknada_km,0) AS naknada_km, 
  IFNULL(pnd.troskovi_goriva,0) AS troskovi_goriva, 
  IFNULL(pnd.troskovi_parkinga,0) AS troskovi_parkinga, 
  IFNULL(pnd.ostali_troskovi,0) AS ostali_troskovi, 
  IFNULL(pnd.obracun_ostalih_troskova,'') AS obr_ostalih_troskova, 
  pnd.order_id
FROM
putninalog_izvjestaj_dod pnd
INNER JOIN putninalog pn ON pnd.pn_id=pn.id
INNER JOIN putninalog_status pns ON pn.status_naloga=pns.id
INNER JOIN evd_employees e ON pn.employee_id=e.EmployeeID
INNER JOIN %__tmp_epotpisi__% AS pot ON pn.id=pot.id
LEFT JOIN sfr_organizacija so ON e.DepartmentUP=so.id
LEFT JOIN putninalog_user pnu ON pn.id=pnu.pn_id
LEFT JOIN my_aspnet_users_to_employees u2e ON pnu.user_id-1=u2e.users_id
WHERE pns.status_text IN(%__StatusText__%)
%__CreatedBy__%
ORDER BY pnd.pn_id, pnd.pn_oblast, pnd.order_id;

    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            strSQL = strSQL.Replace("%__tmp_epotpisi__%", pTmpTableName)
            strSQL = strSQL.Replace("%__ListOrgJed__%", pLstOrg)
            strSQL = strSQL.Replace("%__Days__%", pDays)
            strSQL = strSQL.Replace("%__StatusText__%", pStatusTxt)
            strSQL = strSQL.Replace("%__EmployeeId__%", pEmployeeId)

            strSQL = IIf(pCreatedBy > 0, strSQL.Replace("%__CreatedBy__%", " AND u2e.employees_id = " + pCreatedBy.ToString), strSQL.Replace("%__CreatedBy__%", ""))
            mycmd.CommandText = strSQL

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)


            Me.PnPutniNalogIzvDetList = JsonConvert.DeserializeObject(Of List(Of NgPnPutniNalogDet))(GetJson(DtList))

        End Using

    End Sub

    ' API L9i
    ' byorg + bypg
    Public Sub getPnOrgPGPutniNalogIzvDet(ByVal pLstOrg As String, ByVal pLstPG As String, ByVal pTmpTableName As String, ByVal pStatusTxt As String,
                                     ByVal pEmployeeId As Integer,
                                    Optional pDays As Integer = 30,
                                  Optional pCreatedBy As Integer = -1)

        '
        ' Uzmi org.id iz sys_usr_role_json tabele za usera koji ima email: my_aspnet_membership.Email
        '
        'Dim _lstOrg As String = getOrgJed(pEmail)


        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL, strSQLStart, strSQLOrg, strSQLPG, strSQLEnd As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQLStart = <![CDATA[ 
DROP TABLE IF EXISTS %__tmp_epotpisi__%;

CREATE TEMPORARY TABLE %__tmp_epotpisi__% (
  id INT(11) NOT NULL DEFAULT '0',
  isSignee INT(11) NOT NULL DEFAULT '0',
  Potpis LONGTEXT CHARACTER SET cp1250 NOT NULL,
  status_date  DATETIME DEFAULT NULL,
  created_by  VARCHAR(100) CHARACTER SET cp1250 DEFAULT NULL,
  PRIMARY KEY (id)
) ENGINE=INNODB DEFAULT CHARSET=latin1;

    ]]>.Value

        strSQLOrg = <![CDATA[ 

INSERT IGNORE INTO %__tmp_epotpisi__%
SELECT
 pn.id, 
 0 AS isSignee,
 IFNULL(GROUP_CONCAT(CONCAT(e.LastName,' ',e.FirstName)),'') AS Potpis
 ,NULL, NULL
FROM
  putninalog pn 
  INNER JOIN putninalog_status pns ON pn.status_naloga=pns.id
  INNER JOIN putninalog_user pnu ON pn.id=pnu.pn_id
  INNER JOIN evd_employees e1 ON pn.employee_id=e1.EmployeeID
  LEFT JOIN putninalog_potpisi pnp ON pn.id=pnp.pn_id
  LEFT JOIN evd_potpisi ep ON pnp.evd_potpisi_id=ep.id
  LEFT JOIN evd_employees e ON ep.employee_id=e.EmployeeID
WHERE e1.DepartmentUP IN(SELECT so.id 
FROM sfr_organizacija so
WHERE so.Sifra IN(%__ListOrgJed__%))
AND  pn.dat_poc_putovanja > DATE_ADD(DATE(NOW()), INTERVAL -%__Days__% DAY)
GROUP BY pn.id;
    ]]>.Value

        strSQLPG = <![CDATA[ 
INSERT IGNORE INTO %__tmp_epotpisi__%
SELECT
 pn.id, 
 0 AS isSignee,
 IFNULL(GROUP_CONCAT(CONCAT(e.LastName,' ',e.FirstName)),'') AS Potpis
 ,NULL, NULL
FROM
  putninalog pn 
  INNER JOIN putninalog_status pns ON pn.status_naloga=pns.id
  INNER JOIN putninalog_user pnu ON pn.id=pnu.pn_id
  INNER JOIN evd_employees e1 ON pn.employee_id=e1.EmployeeID
  LEFT JOIN putninalog_potpisi pnp ON pn.id=pnp.pn_id
  LEFT JOIN evd_potpisi ep ON pnp.evd_potpisi_id=ep.id
  LEFT JOIN evd_employees e ON ep.employee_id=e.EmployeeID
WHERE  e1.EmployeeID  IN(%__ListPGJed__%)
AND  pn.dat_poc_putovanja > DATE_ADD(DATE(NOW()), INTERVAL -%__Days__% DAY)
GROUP BY pn.id;
    ]]>.Value


        strSQLEnd = <![CDATA[ 
UPDATE   %__tmp_epotpisi__% pn 
  INNER JOIN putninalog_potpisi pnp ON pn.id=pnp.pn_id
  INNER JOIN evd_potpisi ep ON pnp.evd_potpisi_id=ep.id
  INNER JOIN evd_employees e ON ep.employee_id=e.EmployeeID
SET pn.isSignee = 1
WHERE e.EmployeeID=%__EmployeeId__%;

UPDATE   %__tmp_epotpisi__% pn 
  INNER JOIN putninalog_potpisi pnp ON pn.id=pnp.pn_id
  INNER JOIN evd_potpisi ep ON pnp.evd_potpisi_id=ep.id
  INNER JOIN evd_potpisi_status eps ON ep.id=eps.evd_potpisi_id
  INNER JOIN evd_employees e ON ep.employee_id=e.EmployeeID
SET pn.isSignee = 2
WHERE e.EmployeeID=%__EmployeeId__%;


UPDATE    %__tmp_epotpisi__% t1, 
(
SELECT pno.pn_id AS pn_id
,MAX(pno.dummytimestamp) AS dt
FROM putninalog_odobravanje pno
LEFT JOIN putninalog pn ON pno.pn_id=pn.id
AND  pn.dat_poc_putovanja > DATE_ADD(DATE(NOW()), INTERVAL -%__Days__% DAY)
GROUP BY pno.pn_id
ORDER BY pno.pn_id ASC
) t2
SET t1.status_date = t2.dt
WHERE t1.id = t2.pn_id;

UPDATE  %__tmp_epotpisi__% pn 
INNER JOIN putninalog_user pnu ON pnu.pn_id=pn.id 
LEFT JOIN my_aspnet_users mu ON pnu.user_id=mu.id 
LEFT JOIN my_aspnet_users_to_employees u2e ON u2e.users_id+1=mu.id 
LEFT JOIN evd_employees e ON u2e.employees_id=e.EmployeeID 
SET pn.created_by = CONCAT(e.FirstName,' ',e.LastName); 


SELECT
  pnd.id, 
  pnd.pn_id, 
  pnd.pn_oblast, 
  IFNULL(pnd.relacija,'') AS relacija, 
 DATE_FORMAT(TIMESTAMP(IFNULL(pnd.dat_polaska,'0001-01-01'),IFNULL(pnd.vri_polaska,'00:00')),'%Y-%m-%dT%H:%i:%s') AS dat_polaska,   
  DATE_FORMAT(TIMESTAMP(IFNULL(pnd.dat_povratka,'0001-01-01'),IFNULL(pnd.vri_povratka,'00:00')),'%Y-%m-%dT%H:%i:%s') AS dat_povratka, 
  IFNULL(pnd.broj_sati,0) AS broj_sati, 
  IFNULL(pnd.broj_dnevnica, 0) AS broj_dnevnica, 
  IFNULL(pnd.iznos_dnevnice,0) AS iznos_dnevnice, 
  IFNULL(pnd.ukupan_iznos,0) AS ukupan_iznos, 
  IFNULL(pnd.vrsta_prevoza,'') AS vrsta_prevoza, 
  IFNULL(pnd.razred,'') AS razred, 
  IFNULL(pnd.iznos_karte,0) AS iznos_karte, 
  IFNULL(pnd.naknada_km,0) AS naknada_km, 
  IFNULL(pnd.troskovi_goriva,0) AS troskovi_goriva, 
  IFNULL(pnd.troskovi_parkinga,0) AS troskovi_parkinga, 
  IFNULL(pnd.ostali_troskovi,0) AS ostali_troskovi, 
  IFNULL(pnd.obracun_ostalih_troskova,'') AS obr_ostalih_troskova, 
  pnd.order_id
FROM
putninalog_izvjestaj_dod pnd
INNER JOIN putninalog pn ON pnd.pn_id=pn.id
INNER JOIN putninalog_status pns ON pn.status_naloga=pns.id
INNER JOIN evd_employees e ON pn.employee_id=e.EmployeeID
INNER JOIN %__tmp_epotpisi__% AS pot ON pn.id=pot.id
LEFT JOIN sfr_organizacija so ON e.DepartmentUP=so.id
LEFT JOIN putninalog_user pnu ON pn.id=pnu.pn_id
LEFT JOIN my_aspnet_users_to_employees u2e ON pnu.user_id-1=u2e.users_id
WHERE pns.status_text IN(%__StatusText__%)
%__CreatedBy__%
ORDER BY pnd.pn_id, pnd.pn_oblast, pnd.order_id;

    ]]>.Value


        strSQL = <![CDATA[ 
    ]]>.Value


        If pLstOrg.Length > 0 AndAlso pLstPG.Length > 0 Then
            strSQL = strSQLStart + strSQLOrg + strSQLPG + strSQLEnd
        ElseIf pLstOrg.Length > 0 AndAlso pLstPG.Length = 0 Then
            strSQL = strSQLStart + strSQLOrg + strSQLEnd
        ElseIf pLstOrg.Length = 0 AndAlso pLstPG.Length > 0 Then
            strSQL = strSQLStart + strSQLPG + strSQLEnd
        ElseIf pLstOrg.Length = 0 AndAlso pLstPG.Length = 0 Then
            strSQL = strSQLStart + strSQLEnd
        End If

        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            strSQL = strSQL.Replace("%__tmp_epotpisi__%", pTmpTableName)
            strSQL = strSQL.Replace("%__ListOrgJed__%", pLstOrg)
            strSQL = strSQL.Replace("%__ListPGJed__%", pLstPG)
            strSQL = strSQL.Replace("%__Days__%", pDays)
            strSQL = strSQL.Replace("%__StatusText__%", pStatusTxt)
            strSQL = strSQL.Replace("%__EmployeeId__%", pEmployeeId)

            strSQL = IIf(pCreatedBy > 0, strSQL.Replace("%__CreatedBy__%", " AND u2e.employees_id = " + pCreatedBy.ToString),
                         strSQL.Replace("%__CreatedBy__%", ""))
            mycmd.CommandText = strSQL

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)


            Me.PnPutniNalogIzvDetList = JsonConvert.DeserializeObject(Of List(Of NgPnPutniNalogDet))(GetJson(DtList))

        End Using

    End Sub

    '
    ' E-POTPIS funkcije
    '

    Public Function isSignedByPos(ByVal pPnId As Integer, ByVal pPnDoc As Integer, ByVal pSignPos As Integer) As Integer

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim _retVal As Integer = 0


        strSQL = <![CDATA[ 
SELECT COUNT(pnd.`id`) AS total
FROM `putninalog_doc_documents` pnd
WHERE pnd.`pn_id` = @pPnId
AND pnd.`pn_doc` = @pPnDoc
AND pnd.`pn_potpis` = @pSignPos;
    ]]>.Value

        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand
            Dim dbRead As MySqlDataReader

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@pPnId", pPnId)
            mycmd.Parameters.AddWithValue("@pPnDoc", pPnDoc)
            mycmd.Parameters.AddWithValue("@pSignPos", pSignPos)
            mycmd.Prepare()

            Try
                dbRead = mycmd.ExecuteReader
                If dbRead.HasRows Then
                    While dbRead.Read()
                        _retVal = dbRead.GetInt16("total")
                    End While

                    Return _retVal
                End If
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                Return -1
            End Try

        End Using

        Return _retVal

    End Function

    Public Function isSignedBySigner(ByVal pPnId As Integer, ByVal pPnDoc As Integer, ByVal pSignerId As Integer) As Integer

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim _retVal As Integer = 0


        strSQL = <![CDATA[ 
SELECT COUNT(pnd.`id`) AS total
FROM `putninalog_doc_documents` pnd
WHERE pnd.`pn_id` = @pPnId
AND pnd.`pn_doc` = @pPnDoc
AND pnd.`evd_potpisi_id` = @pSignerId;
    ]]>.Value

        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand
            Dim dbRead As MySqlDataReader

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@pPnId", pPnId)
            mycmd.Parameters.AddWithValue("@pPnDoc", pPnDoc)
            mycmd.Parameters.AddWithValue("@pSignerId", pSignerId)
            mycmd.Prepare()

            Try
                dbRead = mycmd.ExecuteReader
                If dbRead.HasRows Then
                    While dbRead.Read()
                        _retVal = dbRead.GetInt16("total")
                    End While

                    Return _retVal
                End If
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                Return -1
            End Try

        End Using

        Return _retVal

    End Function







    Public Function getLastSignedFile(ByVal pPnId As Integer, ByVal pPnDoc As Integer) As String

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String



        strSQL = <![CDATA[ 
SELECT pnd.id, pnd.`pn_potpis`, pnd.`filename`, pnd.`filepath`
FROM `putninalog_doc_documents` pnd
WHERE pnd.`pn_id` = @PnId
AND pnd.`pn_doc` = @pPnDoc
ORDER BY pnd.id DESC LIMIT 0,1;
    ]]>.Value

        Dim _fileName As String = ""
        Dim _filePath As String = ""

        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand
            Dim dbRead As MySqlDataReader

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@PnId", pPnId)
            mycmd.Parameters.AddWithValue("@pPnDoc", pPnDoc)
            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader
            If dbRead.HasRows Then
                While dbRead.Read()
                    _fileName = dbRead.GetString("filename")
                    _filePath = dbRead.GetString("filepath")
                End While

                Return HttpContext.Current.Server.MapPath(_filePath) + _fileName
            End If

        End Using

        Return ""

    End Function

    Public Function getLastSigned(ByVal pPnId As Integer, ByVal pPnDoc As Integer) As ngEmail

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String



        strSQL = <![CDATA[ 
SELECT pnd.id, pnd.`pn_potpis`, pnd.`filename`, pnd.`filepath`, ep.`employee_id`, e.`FirstName`, e.`LastName`, m.`Email`
FROM `putninalog_doc_documents` pnd
INNER JOIN `evd_potpisi` ep ON pnd.`evd_potpisi_id` = ep.`id`
INNER JOIN `evd_employees` e ON ep.`employee_id` = e.`EmployeeID`
INNER JOIN `my_aspnet_users_to_employees` u2e ON e.`EmployeeID` = u2e.`employees_id`
INNER JOIN `my_aspnet_membership` m ON u2e.`users_id` = m.`userId`
WHERE pnd.`pn_id` = @PnId
AND pnd.`pn_doc` = @pPnDoc
ORDER BY pnd.id DESC LIMIT 0,1;
    ]]>.Value

        Dim _fileName As String = ""
        Dim _filePath As String = ""

        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand
            Dim dbRead As MySqlDataReader

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@PnId", pPnId)
            mycmd.Parameters.AddWithValue("@pPnDoc", pPnDoc)
            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader
            If dbRead.HasRows Then
                Dim _ngEmail As New ngEmail
                While dbRead.Read()
                    _ngEmail.name = dbRead.GetString("FirstName") + " " + dbRead.GetString("LastName")
                    _ngEmail.email = dbRead.GetString("Email")
                    _ngEmail.employeeID = dbRead.GetString("employee_id")

                    _fileName = dbRead.GetString("filename")
                    _filePath = dbRead.GetString("filepath")
                End While

                _ngEmail.attachPath = HttpContext.Current.Server.MapPath(_filePath) + _fileName

                Return _ngEmail
            End If

        End Using

        Return Nothing

    End Function

    Public Function getLastSignedPos(ByVal pPnId As Integer, ByVal pPnDoc As Integer) As Integer

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String



        strSQL = <![CDATA[ 
SELECT pnd.id, pnd.`pn_potpis`
FROM `putninalog_doc_documents` pnd
WHERE pnd.`pn_id` = @PnId
AND pnd.`pn_doc` = @pPnDoc
ORDER BY pnd.id DESC LIMIT 0,1;
    ]]>.Value

        Dim _retVal As Integer = Nothing

        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand
            Dim dbRead As MySqlDataReader

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@PnId", pPnId)
            mycmd.Parameters.AddWithValue("@pPnDoc", pPnDoc)
            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader
            If dbRead.HasRows Then
                While dbRead.Read()
                    _retVal = dbRead.GetInt16("pn_potpis")
                End While
            End If

        End Using

        Return _retVal

    End Function

    Public Function getLastSignedESignID(ByVal pPnId As Integer, ByVal pPnDoc As Integer) As Integer

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String



        strSQL = <![CDATA[ 
SELECT pnd.id, pnd.`evd_potpisi_id`
FROM `putninalog_doc_documents` pnd
WHERE pnd.`pn_id` = @PnId
AND pnd.`pn_doc` = @pPnDoc
ORDER BY pnd.id DESC LIMIT 0,1;
    ]]>.Value

        Dim _retVal As Integer = Nothing

        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand
            Dim dbRead As MySqlDataReader

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@PnId", pPnId)
            mycmd.Parameters.AddWithValue("@pPnDoc", pPnDoc)
            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader
            If dbRead.HasRows Then
                While dbRead.Read()
                    _retVal = dbRead.GetInt16("evd_potpisi_id")
                End While
            End If

        End Using

        Return _retVal

    End Function

    Public Function getSignedPos(ByVal pPnId As Integer, ByVal pPnDoc As Integer) As Dictionary(Of Integer, Integer)

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        Dim _epotpisId As New Dictionary(Of Integer, Integer)

        strSQL = <![CDATA[ 
SELECT pnp.pn_potpis, pnp.evd_potpisi_id
FROM putninalog_potpisi pnp
WHERE pnp.pn_id = @PnId AND pnp.pn_doc = @pPnDoc;
    ]]>.Value

        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand
            Dim dbRead As MySqlDataReader

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@PnId", pPnId)
            mycmd.Parameters.AddWithValue("@pPnDoc", pPnDoc)
            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader
            If dbRead.HasRows Then
                While dbRead.Read()
                    _epotpisId.Add(dbRead.GetInt16("pn_potpis"), dbRead.GetInt16("evd_potpisi_id"))
                End While
            End If

        End Using

        Return _epotpisId

    End Function


    ' API C1
    Public Function insPutNalNalog(ByRef pPutNalNalog As ngPnPutniNalog) As Integer

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim _RowsAffected As Integer = -1

        strSQL = <![CDATA[
INSERT INTO putninalog ( employee_id, status_naloga)
VALUES ( @employee_id, 0 );
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@employee_id", pPutNalNalog.employee_id)
            mycmd.Prepare()

            Try
                _RowsAffected = mycmd.ExecuteNonQuery()

                If _RowsAffected > 0 Then
                    Return mycmd.LastInsertedId
                End If

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return -1

    End Function

    ' API C1 id putnog naloga u putninalog_user
    Public Function insPutNalNalogUser(ByRef pPutNalNalog As ngPnPutniNalog, ByVal pUserEmail As String) As Integer

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim _RowsAffected As Integer = -1

        strSQL = <![CDATA[
INSERT INTO putninalog_user ( pn_id, user_id)
SELECT @putninalog_id, m.userId+1 FROM my_aspnet_membership m
WHERE m.Email=@user_email;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@putninalog_id", pPutNalNalog.id)
            mycmd.Parameters.AddWithValue("@user_email", pUserEmail)
            mycmd.Prepare()

            Try
                _RowsAffected = mycmd.ExecuteNonQuery()

                If _RowsAffected > 0 Then
                    Return mycmd.LastInsertedId
                End If

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return -1

    End Function


    ' API C1 Dodavanje redova za obračun
    Public Function insPutNalNalogDet(ByRef pPutNalNalogID As Integer) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim _RowsAffected As Integer = -1


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As MySqlCommand = myconnection.CreateCommand()



            Try
                mycmd.CommandText = <![CDATA[
INSERT INTO putninalog_obracun ( pn_id, pn_oblast, order_id)
VALUES (@pPutNalNalog, 1, 1);
    ]]>.Value

                mycmd.Parameters.AddWithValue("@pPutNalNalog", pPutNalNalogID)
                mycmd.Prepare()

                _RowsAffected = mycmd.ExecuteNonQuery()

                mycmd.CommandText = <![CDATA[
INSERT INTO putninalog_obracun ( pn_id, pn_oblast, order_id)
VALUES (@pPutNalNalog, 2, 1);
    ]]>.Value

                mycmd.Prepare()

                _RowsAffected = mycmd.ExecuteNonQuery() + _RowsAffected

                mycmd.CommandText = <![CDATA[
INSERT INTO putninalog_obracun ( pn_id, pn_oblast, order_id)
VALUES (@pPutNalNalog, 3, 1);
    ]]>.Value

                mycmd.Prepare()

                _RowsAffected = mycmd.ExecuteNonQuery() + _RowsAffected

                mycmd.CommandText = <![CDATA[
INSERT INTO putninalog_obracun ( pn_id, pn_oblast, order_id)
VALUES (@pPutNalNalog, 4, 1);
    ]]>.Value

                mycmd.Prepare()

                _RowsAffected = mycmd.ExecuteNonQuery() + _RowsAffected


            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try


            If _RowsAffected > 0 Then
                Return True
            End If

        End Using

        Return False

    End Function

    ' API C1 Dodavanje redova za izvještaj
    Public Function insPutNalNalogIzv(ByRef pPutNalNalogID As Integer) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim _RowsAffected As Integer = -1

        strSQL = <![CDATA[
INSERT INTO putninalog_izvjestaj (pn_id)
VALUES (@pPutNalNalog);
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pPutNalNalog", pPutNalNalogID)
            mycmd.Prepare()

            Try
                _RowsAffected = mycmd.ExecuteNonQuery()

                If _RowsAffected > 0 Then
                    Return True
                End If

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False

    End Function

    ' API C1 Dodavanje redova za izvještaj
    Public Function insPutNalNalogDetIzv(ByRef pPutNalID As Integer) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim _RowsAffected As Integer = -1


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As MySqlCommand = myconnection.CreateCommand()

            'mycmd.Connection = myconnection


            Try

                mycmd.CommandText = <![CDATA[
INSERT INTO putninalog_izvjestaj_dod ( pn_id, pn_oblast, relacija, ostali_troskovi, obracun_ostalih_troskova, order_id)
VALUES (@pPutNalNalog, 1, 'polazak-povratak', 0, NULL, 0);
    ]]>.Value

                mycmd.Parameters.AddWithValue("@pPutNalNalog", pPutNalID)
                mycmd.Prepare()

                _RowsAffected = mycmd.ExecuteNonQuery()

                mycmd.CommandText = <![CDATA[
INSERT INTO putninalog_izvjestaj_dod ( pn_id, pn_oblast, relacija, obracun_ostalih_troskova, order_id)
VALUES (@pPutNalNalog, 4, '***', 'Putarina', 1);
    ]]>.Value

                'mycmd.Parameters.AddWithValue("@pPutNalNalog", pPutNalID)
                mycmd.Prepare()

                _RowsAffected = mycmd.ExecuteNonQuery() + _RowsAffected

                mycmd.CommandText = <![CDATA[
INSERT INTO putninalog_izvjestaj_dod ( pn_id, pn_oblast, relacija, obracun_ostalih_troskova, order_id)
VALUES (@pPutNalNalog, 4, '***', 'Noćenje', 2);
    ]]>.Value

                'mycmd.Parameters.AddWithValue("@pPutNalNalog", pPutNalID)
                mycmd.Prepare()

                _RowsAffected = mycmd.ExecuteNonQuery() + _RowsAffected

                mycmd.CommandText = <![CDATA[
INSERT INTO putninalog_izvjestaj_dod ( pn_id, pn_oblast, relacija, obracun_ostalih_troskova, order_id)
VALUES (@pPutNalNalog, 4, '***', 'Parking', 3);
    ]]>.Value

                'mycmd.Parameters.AddWithValue("@pPutNalNalog", pPutNalID)
                mycmd.Prepare()

                _RowsAffected = mycmd.ExecuteNonQuery() + _RowsAffected

                mycmd.CommandText = <![CDATA[
INSERT INTO putninalog_izvjestaj_dod ( pn_id, pn_oblast, relacija, obracun_ostalih_troskova, order_id)
VALUES (@pPutNalNalog, 4, '***', 'Gorivo', 4);
    ]]>.Value

                'mycmd.Parameters.AddWithValue("@pPutNalNalog", pPutNalID)
                mycmd.Prepare()

                _RowsAffected = mycmd.ExecuteNonQuery() + _RowsAffected

                mycmd.CommandText = <![CDATA[
INSERT INTO putninalog_izvjestaj_dod ( pn_id, pn_oblast, relacija, obracun_ostalih_troskova, order_id)
VALUES (@pPutNalNalog, 4, '***', 'Ostalo', 5);
    ]]>.Value

                'mycmd.Parameters.AddWithValue("@pPutNalNalog", pPutNalID)
                mycmd.Prepare()

                _RowsAffected = mycmd.ExecuteNonQuery() + _RowsAffected

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try


            If _RowsAffected > 0 Then
                Return True
            End If

        End Using

        Return False

    End Function

    ' API S1
    Public Function updPutNalStatus(ByVal pPutNalID As Integer, ByVal pStatus As String, ByVal pUserEmpId As Integer) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As MySqlCommand = myconnection.CreateCommand()

            Try


                mycmd.CommandText = <![CDATA[
UPDATE `putninalog` pn, `putninalog_status` pns 
SET pn.`status_naloga` = pns.id
WHERE pn.id = @pPutNalID
AND pns.status_text = @pStatus; 
    ]]>.Value


                mycmd.Parameters.AddWithValue("@pPutNalID", pPutNalID)
                mycmd.Parameters.AddWithValue("@pStatus", pStatus)
                mycmd.Parameters.AddWithValue("@pUserEmpId", pUserEmpId)

                mycmd.Prepare()
                mycmd.ExecuteNonQuery()

                mycmd.CommandText = <![CDATA[
INSERT INTO putninalog_odobravanje (
   pn_id, status_id, employee_id
)
SELECT pn.id, pns.id, @pUserEmpId
FROM putninalog pn, putninalog_status pns 
WHERE pn.id = @pPutNalID
AND pns.status_text = @pStatus; 
    ]]>.Value

                mycmd.Prepare()
                mycmd.ExecuteNonQuery()

                Return True
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False

    End Function

    Public Function insPutNalLog(ByVal pPutNalID As Integer, ByVal pUserEmpId As Integer, ByVal pPnDoc As Integer, ByVal pSignPos As Integer, ByVal pEmpPotpisId As Integer) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        strSQL = <![CDATA[
INSERT INTO putninalog_odobravanje (
  pn_id,
  employee_id,
  pn_doc,
  pn_potpis,
  evd_potpisi_id
)
SELECT
  @pPutNalID,
  @pUserEmpId,
  @pPnDoc,
  @pSignPos,
  @pEmpPotpisId;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pPutNalID", pPutNalID)
            mycmd.Parameters.AddWithValue("@pUserEmpId", pUserEmpId)
            mycmd.Parameters.AddWithValue("@pPnDoc", pPnDoc)
            mycmd.Parameters.AddWithValue("@pSignPos", pSignPos)
            mycmd.Parameters.AddWithValue("@pEmpPotpisId", pEmpPotpisId)
            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
                Return True
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False

    End Function

    ' API S1
    Public Function updPutNalAkontacija(ByVal pPutNalID As Integer, ByVal pIsplAkontacija As Decimal) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        strSQL = <![CDATA[
            UPDATE `putninalog` pn
SET pn.`ispl_akontacije` = @ispl_akontacije
WHERE pn.id = @id; 

    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@id", pPutNalID)
            mycmd.Parameters.AddWithValue("@ispl_akontacije", pIsplAkontacija)
            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
                Return True
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False

    End Function

    ' protokolevid API poziva
    Public Shared Function updPutNalProtokol(ByVal pPutNalID As Integer) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        strSQL = <![CDATA[
UPDATE
  `putninalog` pn,
  `putninalog_evidencija` pne
SET
  pn.`org_jed` = CONCAT(
    IFNULL(pne.pn_protokol, ''),
    ' / ',
    IFNULL(pne.`pn_redbr`, ''),
    '-',
    IFNULL(pne.`pn_mjesec`, ''),
    ' / ',
    IFNULL(pne.`pn_godina`, '')
  )
WHERE 
pn.`id` = pne.`pn_id` AND
pn.id = @id; 

    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@id", pPutNalID)
            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
                Return True
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False

    End Function

    ' API S2
    Public Function updPutNalNalog(ByVal pPutNalID As Integer, ByRef pPutNalNalog As ngPnPutniNalog) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        strSQL = <![CDATA[
UPDATE
  putninalog
SET 
  employee_id = @employee_id, 
  ime_prezime = @ime_prezime, 
  radno_mjesto = @radno_mjesto, 
  mjesto_start = @mjesto_start, 
  mjesto_putovanja = @mjesto_putovanja, 
  razlog_putovanja = @razlog_putovanja, 
  dat_poc_putovanja = @dat_poc_putovanja, 
  trajanje_putovanja = @trajanje_putovanja, 
  relacija = @relacija, 
  prev_text = @prev_text, 
  prev_auto = @prev_auto, 
  prev_vlauto = @prev_vlauto, 
  prev_avion = @prev_avion, 
  prev_autobus = @prev_autobus, 
  prevoz_voz = @prevoz_voz, 
  troskovi_putovanja_knjizenje = @troskovi_putovanja_knjizenje, 
  aktivnost = @aktivnost,
  org_jed =  @org_jed, 
  ukupnitroskovi_a = @ukupnitroskovi_a, 
  ukupnitroskovi_b = @ukupnitroskovi_b, 
  ukupnitroskovi_c = @ukupnitroskovi_c,
  ukupnitroskovi_d = @ukupnitroskovi_d,
  iznos_obracuna = @iznos_obracuna, 
  iznos_razlika = @iznos_razlika,
  iznos_dnevnice = @iznos_dnevnice, 
  proc_dnevnice = @proc_dnevnice, 
  broj_dnevnica = @broj_dnevnica, 
  akontacija_dnevnice = @akontacija_dnevnice, 
  akontacija_nocenje = @akontacija_nocenje,  
  akontacija_ostalo = @akontacija_ostalo, 
  iznos_akontacije = @iznos_akontacije,
  ispl_akontacije = @ispl_akontacije,
  odobrena_akontacija = @odobrena_akontacija,
  vrsta_naloga = @vrsta_naloga
WHERE id = @pPutNalID;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pPutNalID", pPutNalID)

            mycmd.Parameters.AddWithValue("@employee_id", pPutNalNalog.employee_id)
            mycmd.Parameters.AddWithValue("@ime_prezime", pPutNalNalog.ime_prezime)
            mycmd.Parameters.AddWithValue("@radno_mjesto", pPutNalNalog.radno_mjesto)
            mycmd.Parameters.AddWithValue("@mjesto_start", pPutNalNalog.mjesto_start)
            mycmd.Parameters.AddWithValue("@mjesto_putovanja", pPutNalNalog.mjesto_putovanja)
            mycmd.Parameters.AddWithValue("@razlog_putovanja", pPutNalNalog.razlog_putovanja)
            mycmd.Parameters.AddWithValue("@dat_poc_putovanja", pPutNalNalog.dat_poc_putovanja)
            mycmd.Parameters.AddWithValue("@trajanje_putovanja", pPutNalNalog.trajanje_putovanja)
            mycmd.Parameters.AddWithValue("@relacija", pPutNalNalog.relacija)
            mycmd.Parameters.AddWithValue("@prev_text", pPutNalNalog.prev_text)
            mycmd.Parameters.AddWithValue("@prev_auto", pPutNalNalog.PrevAuto)
            mycmd.Parameters.AddWithValue("@prev_vlauto", pPutNalNalog.PrevVlAuto)
            mycmd.Parameters.AddWithValue("@prev_avion", pPutNalNalog.PrevAvion)
            mycmd.Parameters.AddWithValue("@prev_autobus", pPutNalNalog.PrevAutobus)
            mycmd.Parameters.AddWithValue("@prevoz_voz", pPutNalNalog.PrevVoz)
            mycmd.Parameters.AddWithValue("@troskovi_putovanja_knjizenje", pPutNalNalog.troskovi_putovanja_knjizenje)
            mycmd.Parameters.AddWithValue("@aktivnost", pPutNalNalog.aktivnost)
            mycmd.Parameters.AddWithValue("@org_jed", pPutNalNalog.org_jed)
            mycmd.Parameters.AddWithValue("@ukupnitroskovi_a", pPutNalNalog.UkTroskoviA)
            mycmd.Parameters.AddWithValue("@ukupnitroskovi_b", pPutNalNalog.UkTroskoviB)
            mycmd.Parameters.AddWithValue("@ukupnitroskovi_c", pPutNalNalog.UkTroskoviC)
            mycmd.Parameters.AddWithValue("@ukupnitroskovi_d", pPutNalNalog.UkTroskoviD)
            mycmd.Parameters.AddWithValue("@iznos_obracuna", pPutNalNalog.UkIznosObracuna)
            mycmd.Parameters.AddWithValue("@iznos_razlika", pPutNalNalog.UkIznosRazlika)
            mycmd.Parameters.AddWithValue("@proc_dnevnice", pPutNalNalog.proc_dnevnice)
            mycmd.Parameters.AddWithValue("@iznos_dnevnice", pPutNalNalog.iznos_dnevnice)
            mycmd.Parameters.AddWithValue("@broj_dnevnica", pPutNalNalog.broj_dnevnica)
            mycmd.Parameters.AddWithValue("@akontacija_dnevnice", pPutNalNalog.akontacija_dnevnice)
            mycmd.Parameters.AddWithValue("@akontacija_nocenje", pPutNalNalog.akontacija_nocenje)
            mycmd.Parameters.AddWithValue("@akontacija_ostalo", pPutNalNalog.akontacija_ostalo)
            mycmd.Parameters.AddWithValue("@iznos_akontacije", pPutNalNalog.iznos_akontacije)
            mycmd.Parameters.AddWithValue("@ispl_akontacije", pPutNalNalog.ispl_akontacije)
            mycmd.Parameters.AddWithValue("@odobrena_akontacija", pPutNalNalog.odobrena_akontacija)
            mycmd.Parameters.AddWithValue("@vrsta_naloga", pPutNalNalog.vrsta_naloga)

            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
                Return True
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False

    End Function

    ' API S2A
    Public Function updPutNalIzv(ByVal pPutNalID As Integer, ByRef pPutNalNalog As ngPnPutniNalog) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        strSQL = <![CDATA[
UPDATE putninalog_izvjestaj
SET pn_izvjestaj = @izv
WHERE pn_id = @pPutNalID;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pPutNalID", pPutNalID)
            mycmd.Parameters.AddWithValue("@izv", pPutNalNalog.izvjestaj)
            mycmd.Prepare()


            Try
                mycmd.ExecuteNonQuery()
                Return True
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False

    End Function

    ' API S3 Sql.Update
    Public Function updPutNalNalogDet(ByVal pID As Integer, ByRef pPutNalNalogDet As NgPnPutniNalogDet) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        strSQL = <![CDATA[
UPDATE
  putninalog_obracun
SET
  pn_oblast = @pn_oblast, 
  relacija = @relacija, 
  dat_polaska = @dat_polaska, 
  vri_polaska = @vri_polaska, 
  dat_povratka = @dat_povratka, 
  vri_povratka = @vri_povratka, 
  broj_sati = @broj_sati, 
  broj_dnevnica = @broj_dnevnica, 
  iznos_dnevnice = @iznos_dnevnice, 
  ukupan_iznos = @ukupan_iznos, 
  vrsta_prevoza = @vrsta_prevoza, 
  razred = @razred, 
  iznos_karte = @iznos_karte, 
  naknada_km = @naknada_km, 
  troskovi_goriva = @troskovi_goriva, 
  troskovi_parkinga = @troskovi_parkinga, 
  ostali_troskovi = @ostali_troskovi, 
  obracun_ostalih_troskova = @obracun_ostalih_troskova, 
  order_id = @order_id
WHERE id = @id AND pn_id = @pn_id;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@id", pID)
            mycmd.Parameters.AddWithValue("@pn_id", pPutNalNalogDet.pn_id)

            mycmd.Parameters.AddWithValue("@pn_oblast", pPutNalNalogDet.pn_oblast)
            mycmd.Parameters.AddWithValue("@relacija", pPutNalNalogDet.relacija)
            mycmd.Parameters.AddWithValue("@dat_polaska", pPutNalNalogDet.dat_polaska)
            mycmd.Parameters.AddWithValue("@vri_polaska", pPutNalNalogDet.dat_polaska)
            mycmd.Parameters.AddWithValue("@dat_povratka", pPutNalNalogDet.dat_povratka)
            mycmd.Parameters.AddWithValue("@vri_povratka", pPutNalNalogDet.dat_povratka)
            mycmd.Parameters.AddWithValue("@broj_sati", pPutNalNalogDet.broj_sati)
            mycmd.Parameters.AddWithValue("@broj_dnevnica", pPutNalNalogDet.broj_dnevnica)
            mycmd.Parameters.AddWithValue("@iznos_dnevnice", pPutNalNalogDet.iznos_dnevnice)
            mycmd.Parameters.AddWithValue("@ukupan_iznos", pPutNalNalogDet.ukupan_iznos)
            mycmd.Parameters.AddWithValue("@vrsta_prevoza", pPutNalNalogDet.vrsta_prevoza)
            mycmd.Parameters.AddWithValue("@razred", pPutNalNalogDet.razred)
            mycmd.Parameters.AddWithValue("@iznos_karte", pPutNalNalogDet.iznos_karte)
            mycmd.Parameters.AddWithValue("@naknada_km", pPutNalNalogDet.naknada_km)
            mycmd.Parameters.AddWithValue("@troskovi_goriva", pPutNalNalogDet.troskovi_goriva)
            mycmd.Parameters.AddWithValue("@troskovi_parkinga", pPutNalNalogDet.troskovi_parkinga)
            mycmd.Parameters.AddWithValue("@ostali_troskovi", pPutNalNalogDet.ostali_troskovi)
            mycmd.Parameters.AddWithValue("@obracun_ostalih_troskova", pPutNalNalogDet.obr_ostalih_troskova)
            mycmd.Parameters.AddWithValue("@order_id", pPutNalNalogDet.order_id)

            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
                Return True
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False
        Return False

    End Function

    ' API S3 Sql.Insert
    Public Function insPutNalNalogDet(ByVal pID As Integer, ByRef pPutNalNalogDet As NgPnPutniNalogDet) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        strSQL = <![CDATA[
INSERT INTO putninalog_obracun (
  pn_id, 
  pn_oblast, 
  relacija, 
  dat_polaska, 
  dat_povratka, 
  vri_polaska,  
  vri_povratka,  
  broj_sati, 
  broj_dnevnica, 
  iznos_dnevnice, 
  ukupan_iznos, 
  vrsta_prevoza, 
  razred, 
  iznos_karte, 
  naknada_km, 
  troskovi_goriva, 
  troskovi_parkinga, 
  ostali_troskovi, 
  obracun_ostalih_troskova, 
  order_id
)
VALUES
  (
  @pn_id,
  @pn_oblast, 
  @relacija, 
  @dat_polaska, 
  @dat_povratka, 
  @vri_polaska,
  @vri_povratka,
  @broj_sati, 
  @broj_dnevnica, 
  @iznos_dnevnice, 
  @ukupan_iznos, 
  @vrsta_prevoza, 
  @razred, 
  @iznos_karte, 
  @naknada_km, 
  @troskovi_goriva, 
  @troskovi_parkinga, 
  @ostali_troskovi, 
  @obracun_ostalih_troskova, 
  @order_id
  );
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pn_id", pPutNalNalogDet.pn_id)
            'mycmd.Parameters.AddWithValue("@id", pPutNalNalogDet.pn_id)

            mycmd.Parameters.AddWithValue("@pn_oblast", pPutNalNalogDet.pn_oblast)
            mycmd.Parameters.AddWithValue("@relacija", pPutNalNalogDet.relacija)
            mycmd.Parameters.AddWithValue("@dat_polaska", pPutNalNalogDet.dat_polaska)
            mycmd.Parameters.AddWithValue("@vri_polaska", pPutNalNalogDet.dat_polaska)
            mycmd.Parameters.AddWithValue("@dat_povratka", pPutNalNalogDet.dat_povratka)
            mycmd.Parameters.AddWithValue("@vri_povratka", pPutNalNalogDet.dat_povratka)
            mycmd.Parameters.AddWithValue("@broj_sati", pPutNalNalogDet.broj_sati)
            mycmd.Parameters.AddWithValue("@broj_dnevnica", pPutNalNalogDet.broj_dnevnica)
            mycmd.Parameters.AddWithValue("@iznos_dnevnice", pPutNalNalogDet.iznos_dnevnice)
            mycmd.Parameters.AddWithValue("@ukupan_iznos", pPutNalNalogDet.ukupan_iznos)
            mycmd.Parameters.AddWithValue("@vrsta_prevoza", pPutNalNalogDet.vrsta_prevoza)
            mycmd.Parameters.AddWithValue("@razred", pPutNalNalogDet.razred)
            mycmd.Parameters.AddWithValue("@iznos_karte", pPutNalNalogDet.iznos_karte)
            mycmd.Parameters.AddWithValue("@naknada_km", pPutNalNalogDet.naknada_km)
            mycmd.Parameters.AddWithValue("@troskovi_goriva", pPutNalNalogDet.troskovi_goriva)
            mycmd.Parameters.AddWithValue("@troskovi_parkinga", pPutNalNalogDet.troskovi_parkinga)
            mycmd.Parameters.AddWithValue("@ostali_troskovi", pPutNalNalogDet.ostali_troskovi)
            mycmd.Parameters.AddWithValue("@obracun_ostalih_troskova", pPutNalNalogDet.obr_ostalih_troskova)
            mycmd.Parameters.AddWithValue("@order_id", pPutNalNalogDet.order_id)

            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
                Return True
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False
        Return False

    End Function

    ' API S3 Suma ukupnih troškova UPDATE
    Public Function updPutNalDetSuma(ByVal pPutNalID As Integer) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        strSQL = <![CDATA[
UPDATE putninalog pn,
(SELECT SUM(pno.ukupan_iznos) AS ukupan_iznos FROM putninalog_obracun pno
WHERE pno.pn_id=@pPutNalID
AND pno.pn_oblast=1) t
SET pn.ukupnitroskovi_a=t.ukupan_iznos
WHERE pn.id=@pPutNalID;

UPDATE putninalog pn,
(SELECT SUM(pno.ukupan_iznos) AS ukupan_iznos FROM putninalog_obracun pno
WHERE pno.pn_id=@pPutNalID
AND pno.pn_oblast=2) t
SET pn.ukupnitroskovi_b=t.ukupan_iznos
WHERE pn.id=@pPutNalID;

UPDATE putninalog pn,
(SELECT SUM(pno.ukupan_iznos) AS ukupan_iznos FROM putninalog_obracun pno
WHERE pno.pn_id=@pPutNalID
AND pno.pn_oblast=3) t
SET pn.ukupnitroskovi_c=t.ukupan_iznos
WHERE pn.id=@pPutNalID;

UPDATE putninalog pn,
(SELECT SUM(pno.ukupan_iznos) AS ukupan_iznos FROM putninalog_obracun pno
WHERE pno.pn_id=@pPutNalID
AND pno.pn_oblast=4) t
SET pn.ukupnitroskovi_d=t.ukupan_iznos
WHERE pn.id=@pPutNalID;

UPDATE putninalog pn
SET pn.iznos_obracuna=IFNULL(pn.ukupnitroskovi_a,0)+IFNULL(pn.ukupnitroskovi_b,0)+
IFNULL(pn.ukupnitroskovi_c,0)+IFNULL(pn.ukupnitroskovi_d,0)
WHERE pn.id=@pPutNalID;

UPDATE putninalog pn
SET pn.iznos_razlika= pn.iznos_obracuna-pn.ispl_akontacije
WHERE pn.id=@pPutNalID;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pPutNalID", pPutNalID)
            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
                Return True
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False

    End Function


    ' API S5i Sql.Update
    Public Function updPutNalNalogIzvDet(ByVal pID As Integer, ByRef pPutNalNalogDet As NgPnPutniNalogDet) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        strSQL = <![CDATA[
UPDATE
  putninalog_izvjestaj_dod
SET
  pn_oblast = @pn_oblast, 
  relacija = @relacija, 
  dat_polaska = @dat_polaska, 
  vri_polaska = @vri_polaska, 
  dat_povratka = @dat_povratka, 
  vri_povratka = @vri_povratka, 
  broj_sati = @broj_sati, 
  broj_dnevnica = @broj_dnevnica, 
  iznos_dnevnice = @iznos_dnevnice, 
  ukupan_iznos = @ukupan_iznos, 
  vrsta_prevoza = @vrsta_prevoza, 
  razred = @razred, 
  iznos_karte = @iznos_karte, 
  naknada_km = @naknada_km, 
  troskovi_goriva = @troskovi_goriva, 
  troskovi_parkinga = @troskovi_parkinga, 
  ostali_troskovi = @ostali_troskovi, 
  obracun_ostalih_troskova = @obracun_ostalih_troskova, 
  order_id = @order_id
WHERE id = @id AND pn_id = @pn_id;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@id", pID)
            mycmd.Parameters.AddWithValue("@pn_id", pPutNalNalogDet.pn_id)

            mycmd.Parameters.AddWithValue("@pn_oblast", pPutNalNalogDet.pn_oblast)
            mycmd.Parameters.AddWithValue("@relacija", pPutNalNalogDet.relacija)
            mycmd.Parameters.AddWithValue("@dat_polaska", pPutNalNalogDet.dat_polaska)
            mycmd.Parameters.AddWithValue("@vri_polaska", pPutNalNalogDet.dat_polaska)
            mycmd.Parameters.AddWithValue("@dat_povratka", pPutNalNalogDet.dat_povratka)
            mycmd.Parameters.AddWithValue("@vri_povratka", pPutNalNalogDet.dat_povratka)
            mycmd.Parameters.AddWithValue("@broj_sati", pPutNalNalogDet.broj_sati)
            mycmd.Parameters.AddWithValue("@broj_dnevnica", pPutNalNalogDet.broj_dnevnica)
            mycmd.Parameters.AddWithValue("@iznos_dnevnice", pPutNalNalogDet.iznos_dnevnice)
            mycmd.Parameters.AddWithValue("@ukupan_iznos", pPutNalNalogDet.ukupan_iznos)
            mycmd.Parameters.AddWithValue("@vrsta_prevoza", pPutNalNalogDet.vrsta_prevoza)
            mycmd.Parameters.AddWithValue("@razred", pPutNalNalogDet.razred)
            mycmd.Parameters.AddWithValue("@iznos_karte", pPutNalNalogDet.iznos_karte)
            mycmd.Parameters.AddWithValue("@naknada_km", pPutNalNalogDet.naknada_km)
            mycmd.Parameters.AddWithValue("@troskovi_goriva", pPutNalNalogDet.troskovi_goriva)
            mycmd.Parameters.AddWithValue("@troskovi_parkinga", pPutNalNalogDet.troskovi_parkinga)
            mycmd.Parameters.AddWithValue("@ostali_troskovi", pPutNalNalogDet.ostali_troskovi)
            mycmd.Parameters.AddWithValue("@obracun_ostalih_troskova", pPutNalNalogDet.obr_ostalih_troskova)
            mycmd.Parameters.AddWithValue("@order_id", pPutNalNalogDet.order_id)

            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
                Return True
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False
        Return False

    End Function

    ' API S5i Sql.Insert
    Public Function insPutNalNalogIzvDet(ByVal pID As Integer, ByRef pPutNalNalogDet As NgPnPutniNalogDet) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        strSQL = <![CDATA[
INSERT INTO putninalog_izvjestaj_dod (
  pn_id, 
  pn_oblast, 
  relacija, 
  dat_polaska, 
  dat_povratka, 
  vri_polaska,  
  vri_povratka,  
  broj_sati, 
  broj_dnevnica, 
  iznos_dnevnice, 
  ukupan_iznos, 
  vrsta_prevoza, 
  razred, 
  iznos_karte, 
  naknada_km, 
  troskovi_goriva, 
  troskovi_parkinga, 
  ostali_troskovi, 
  obracun_ostalih_troskova, 
  order_id
)
VALUES
  (
  @pn_id,
  @pn_oblast, 
  @relacija, 
  @dat_polaska, 
  @dat_povratka, 
  @vri_polaska,
  @vri_povratka,
  @broj_sati, 
  @broj_dnevnica, 
  @iznos_dnevnice, 
  @ukupan_iznos, 
  @vrsta_prevoza, 
  @razred, 
  @iznos_karte, 
  @naknada_km, 
  @troskovi_goriva, 
  @troskovi_parkinga, 
  @ostali_troskovi, 
  @obracun_ostalih_troskova, 
  @order_id
  );
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pn_id", pPutNalNalogDet.pn_id)
            'mycmd.Parameters.AddWithValue("@id", pPutNalNalogDet.pn_id)

            mycmd.Parameters.AddWithValue("@pn_oblast", pPutNalNalogDet.pn_oblast)
            mycmd.Parameters.AddWithValue("@relacija", pPutNalNalogDet.relacija)
            mycmd.Parameters.AddWithValue("@dat_polaska", pPutNalNalogDet.dat_polaska)
            mycmd.Parameters.AddWithValue("@vri_polaska", pPutNalNalogDet.dat_polaska)
            mycmd.Parameters.AddWithValue("@dat_povratka", pPutNalNalogDet.dat_povratka)
            mycmd.Parameters.AddWithValue("@vri_povratka", pPutNalNalogDet.dat_povratka)
            mycmd.Parameters.AddWithValue("@broj_sati", pPutNalNalogDet.broj_sati)
            mycmd.Parameters.AddWithValue("@broj_dnevnica", pPutNalNalogDet.broj_dnevnica)
            mycmd.Parameters.AddWithValue("@iznos_dnevnice", pPutNalNalogDet.iznos_dnevnice)
            mycmd.Parameters.AddWithValue("@ukupan_iznos", pPutNalNalogDet.ukupan_iznos)
            mycmd.Parameters.AddWithValue("@vrsta_prevoza", pPutNalNalogDet.vrsta_prevoza)
            mycmd.Parameters.AddWithValue("@razred", pPutNalNalogDet.razred)
            mycmd.Parameters.AddWithValue("@iznos_karte", pPutNalNalogDet.iznos_karte)
            mycmd.Parameters.AddWithValue("@naknada_km", pPutNalNalogDet.naknada_km)
            mycmd.Parameters.AddWithValue("@troskovi_goriva", pPutNalNalogDet.troskovi_goriva)
            mycmd.Parameters.AddWithValue("@troskovi_parkinga", pPutNalNalogDet.troskovi_parkinga)
            mycmd.Parameters.AddWithValue("@ostali_troskovi", pPutNalNalogDet.ostali_troskovi)
            mycmd.Parameters.AddWithValue("@obracun_ostalih_troskova", pPutNalNalogDet.obr_ostalih_troskova)
            mycmd.Parameters.AddWithValue("@order_id", pPutNalNalogDet.order_id)

            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
                Return True
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False

    End Function

    ' API S6
    Public Function updPutNalNalogCol(ByVal pPutNalID As Integer, ByRef pPutNalNalog As ngPnPutniNalog, ByVal pColName As String) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String


        Select Case pColName
            Case "org_jed"

            Case Else
                Return False
        End Select

        strSQL = <![CDATA[
UPDATE
  putninalog
SET 
  org_jed =  @org_jed
WHERE id = @pPutNalID;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pPutNalID", pPutNalID)
            mycmd.Parameters.AddWithValue("@org_jed", pPutNalNalog.org_jed)
            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
                Return True
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False

    End Function

    ' API S6
    Public Function updPutNalNalogTem(ByVal pPutNalID As Integer, ByRef pPutNalNalog As ngPnPutniNalog) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String


        strSQL = <![CDATA[
REPLACE INTO putninalog_temeljnica
( pn_id, pn_vrsta, pn_temeljnica) 
VALUES
(@pPutNalID, 1, @pTemeljnica1);

REPLACE INTO putninalog_temeljnica
( pn_id, pn_vrsta, pn_temeljnica) 
VALUES
(@pPutNalID, 2, @pTemeljnica2);
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pPutNalID", pPutNalID)

            mycmd.Parameters.AddWithValue("@pTemeljnica1", pPutNalNalog.temeljnica_1)
            mycmd.Parameters.AddWithValue("@pTemeljnica2", pPutNalNalog.temeljnica_2)
            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
                Return True
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False

    End Function


    ' API D1 Sql.Delete
    Public Function delPutNalNalog(ByVal pPutNalID As Integer) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL1, strSQL2 As String
        Dim _RowsAffected As Integer = -1

        strSQL1 = <![CDATA[
DELETE FROM putninalog
WHERE id=@pn_id;
    ]]>.Value

        strSQL2 = <![CDATA[
DELETE FROM putninalog_obracun
WHERE pn_id=@pn_id;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL1


            mycmd.Parameters.AddWithValue("@pn_id", pPutNalID)
            mycmd.Prepare()

            Try
                _RowsAffected = mycmd.ExecuteNonQuery()

                mycmd.CommandText = strSQL2


                mycmd.Parameters.Clear()
                mycmd.Parameters.AddWithValue("@pn_id", pPutNalID)
                mycmd.Prepare()

                _RowsAffected += mycmd.ExecuteNonQuery()

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        If _RowsAffected > 0 Then
            Return True
        Else
            Return False
        End If

    End Function

    ' API D2 Sql.Delete
    Public Function delPutNalNalogDet(ByVal pPutNalID As Integer, ByVal pID As Integer) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        strSQL = <![CDATA[
DELETE
FROM
  putninalog_obracun
WHERE id = @id AND pn_id=@pn_id;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pn_id", pPutNalID)
            mycmd.Parameters.AddWithValue("@id", pID)
            mycmd.Prepare()

            Try
                Dim _RowsAffected = mycmd.ExecuteNonQuery()

                If _RowsAffected > 0 Then
                    Return True
                Else
                    Return False
                End If

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False

    End Function

    ' API D3i Sql.Delete
    Public Function delPutNalNalogIzvDet(ByVal pPutNalID As Integer, ByVal pID As Integer) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        strSQL = <![CDATA[
DELETE
FROM
  putninalog_izvjestaj_dod
WHERE id = @id AND pn_id=@pn_id;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pn_id", pPutNalID)
            mycmd.Parameters.AddWithValue("@id", pID)
            mycmd.Prepare()

            Try
                Dim _RowsAffected = mycmd.ExecuteNonQuery()

                If _RowsAffected > 0 Then
                    Return True
                Else
                    Return False
                End If

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False

    End Function

    ' API eS1
    ''' <summary>
    ''' Provjerava potpis putnog naloga koji je dobijen od klijenta - i poziva metod insPutNalESignature za upis u bazu (ako je dat parametar pIns2Database)
    ''' </summary>
    ''' <param name="pESignature"></param>
    ''' <param name="pIns2Database"></param>
    ''' <returns></returns>
    'Public Function signPutNal(ByRef pESignature As NgESignature,
    '                           ByRef pNgPnPutniNalog As ngPnPutniNalog,
    '                           Optional ByVal pIns2Database As Boolean = False) As Boolean


    '    'Dim _pn As NgPnPutniNalog = getPnPutniNalogById(pESignature.pPutNalID)
    '    Dim _pn As ngPnPutniNalog = pNgPnPutniNalog

    '    ' VAŽNO !!!
    '    ' uzima public_key iz baze i smješta u pESignature
    '    ' ************************************************
    '    Dim _ePublicKeyExist As Boolean = getPublicKey(pESignature)

    '    Dim _retVal As Boolean = False


    '    If pESignature.ePublicKey.Length > 0 Then

    '        'Dim _string As String = _pn.id.ToString + _pn.ime_prezime + _pn.mjesto_putovanja +
    '        '            _pn.dat_poc_putovanja + _pn.trajanje_putovanja + _pn.iznos_akontacije +
    '        '            _pn.employee_id.ToString

    '        '
    '        ' Izračunavanje HASH stringa
    '        ' 
    '        Dim _dtFormat As String = "yyyy-MM-ddTHH:mm:ss"
    '        Dim _string As String = _pn.id.ToString + _pn.employee_id.ToString + _pn.ime_prezime + _pn.mjesto_putovanja +
    '                                _pn.dat_poc_putovanja.ToString(_dtFormat) +
    '                                _pn.trajanje_putovanja.ToString("F1", CultureInfo.InvariantCulture) +
    '                                _pn.iznos_akontacije.ToString("F6", CultureInfo.InvariantCulture)


    '        Dim _sha256 As SHA256 = SHA256.Create()

    '        Dim _pPutNalHashBytes As Byte() = Encoding.UTF8.GetBytes(_string)

    '        Dim _hashBytes2 As Byte()
    '        Using _sha256
    '            _hashBytes2 = _sha256.ComputeHash(_pPutNalHashBytes)
    '        End Using

    '        '
    '        ' uzima e-potpis iz objekta pESignature
    '        '
    '        Dim _eSignatureBytes As Byte() = Convert.FromBase64String(pESignature.pPutNalESignature)

    '        '
    '        ' Provjera potpisa
    '        '
    '        Dim _rRSA As RSACryptoServiceProvider = ngApiRSAKeys.ImportPublicKey(pESignature.ePublicKey)
    '        Using _rRSA
    '            Dim rsaDeformatter = New RSAPKCS1SignatureDeformatter(_rRSA)
    '            rsaDeformatter.SetHashAlgorithm("SHA256")
    '            _retVal = rsaDeformatter.VerifySignature(_hashBytes2, _eSignatureBytes)
    '        End Using
    '        '_retVal = VerifySignature(_ePublicKeyBytes, _hashBytes, _eSignatureBytes)

    '        '
    '        ' ako je potpis u redu, i dat je parametar pIns2Database, upisati u bazu
    '        '
    '        If (pIns2Database = True) And (_retVal = True) Then
    '            '_retVal = insPutNalESignature(pESignature)


    '            'converting the bytes into string
    '            Dim SB As StringBuilder = New StringBuilder
    '            Dim HB As Byte
    '            For Each HB In _hashBytes2
    '                SB.Append(String.Format("{0:X2}", HB))
    '            Next
    '            'HACK
    '            ' System.Text.Encoding.Unicode.GetString(_hashBytes2)

    '            updSignPutNalHASH(_pn.id, SB.ToString(), _string, 1)
    '            _retVal = insPutNalESignature(pESignature)
    '        End If

    '    End If


    '    Return _retVal

    'End Function

    ' OVO KORISTI mPerun KOD API POZIVA
    'Public Function signPutNalByCert(ByRef pESignature As NgESignature,
    '                           ByRef pNgPnPutniNalog As ngPnPutniNalog,
    '                           ByRef pInData As ngPnPotpisByCert,
    '                           Optional ByVal pIns2Database As Boolean = False) As Boolean




    '    'Dim _pn As NgPnPutniNalog = getPnPutniNalogById(pESignature.pPutNalID)
    '    Dim _pn As ngPnPutniNalog = pNgPnPutniNalog

    '    ' VAŽNO !!!
    '    ' uzima public_key iz baze i smješta u pESignature
    '    ' ************************************************
    '    Dim _ePublicKeyExist As Boolean = getPublicKey(pESignature)

    '    Dim _retVal As Boolean = False


    '    If pESignature.ePublicKey.Length > 0 Then

    '        'Dim _string As String = _pn.id.ToString + _pn.ime_prezime + _pn.mjesto_putovanja +
    '        '            _pn.dat_poc_putovanja + _pn.trajanje_putovanja + _pn.iznos_akontacije +
    '        '            _pn.employee_id.ToString

    '        '
    '        ' Izračunavanje HASH stringa
    '        ' 
    '        Dim _dtFormat As String = "yyyy-MM-ddTHH:mm:ss"
    '        Dim _string As String = _pn.id.ToString + _pn.employee_id.ToString + _pn.ime_prezime + _pn.mjesto_putovanja +
    '                                _pn.dat_poc_putovanja.ToString(_dtFormat) +
    '                                _pn.trajanje_putovanja.ToString("F1", CultureInfo.InvariantCulture) +
    '                                _pn.iznos_akontacije.ToString("F6", CultureInfo.InvariantCulture)


    '        Dim _sha256 As SHA256 = SHA256.Create()

    '        Dim _pPutNalHashBytes As Byte() = Encoding.UTF8.GetBytes(_string)

    '        Dim _hashBytes2 As Byte()
    '        Using _sha256
    '            _hashBytes2 = _sha256.ComputeHash(_pPutNalHashBytes)
    '        End Using

    '        '
    '        ' ako je potpis u redu, i dat je parametar pIns2Database, upisati u bazu
    '        '
    '        If (pIns2Database = True) Then
    '            '_retVal = insPutNalESignature(pESignature)


    '            'converting the bytes into string
    '            Dim SB As StringBuilder = New StringBuilder
    '            Dim HB As Byte
    '            For Each HB In _hashBytes2
    '                SB.Append(String.Format("{0:X2}", HB))
    '            Next
    '            'HACK
    '            ' System.Text.Encoding.Unicode.GetString(_hashBytes2)

    '            updSignPutNalHASH(_pn.id, SB.ToString(), _string, 1)
    '            _retVal = insPutNalESignature(pESignature, pInData.pnDoc, pInData.signPos)
    '        End If

    '    End If


    '    Return _retVal

    'End Function


    Public Function signPutNalByCert2(ByRef pESignature As NgESignature,
                               ByVal _stringJSON As String,
                               ByRef pECert As NgECertificate,
                                ByRef pInData As ngPnPotpisByCert,
                               Optional ByVal pIns2Database As Boolean = False) As Boolean

        Dim _ApiCert As New ngApiCertificate()
        _ApiCert.getPfxFromDb(pECert.id)
        _ApiCert.setPrivateKey(pECert)


        Dim _certificate As X509Certificate2 = New X509Certificate2()
        _certificate.Import(_ApiCert.rawData, pECert.password, X509KeyStorageFlags.Exportable Or X509KeyStorageFlags.UserKeySet)
        Console.WriteLine(_certificate.ToString(True))

        Dim _signedJSON As String
        Dim _ApiCertSign As New ngApiCertificateSign()

        Try
            ' Potpisivanje podataka
            _signedJSON = _ApiCertSign.SignData(_stringJSON, pECert)

            ' Verifikacija potpisa pomoću PublicKey koji se uzima iz X509Certicate2
            ' NEMA POTREBE ZA VERIFIKACIJOM POMOĆU CERTIFIKATA => KORISTI SE VERIFIKACIJA POMOĆU PUBLICKEY IZ BAZE

            'Dim _verification As Boolean = _ApiCertSign.VerifySignature(_certificate, _signedJSON, _stringJSON)
            'Console.WriteLine(_verification)

        Catch ex As Exception
            Console.Write(ex.ToString)

            Return False
        End Try

        Dim _retVal As Boolean = False

        ' uzima public_key iz baze i smješta u pESignature
        ' 
        getPublicKey(pESignature)


        If pESignature.ePublicKey.Length > 0 Then

            '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            ' Verifikacija potpisa pomoću PublicKey koji se uzima iz baze

            _retVal = _ApiCertSign.VerifySignature2(pECert, pESignature.ePublicKey, _signedJSON, _stringJSON)

            '
            If (pIns2Database = True) And (_retVal = True) Then
                '_retVal = insPutNalESignature(pESignature)


                'converting the bytes into string
                Dim _hashBytesJSON As Byte() = ngApiUtilities.getHash_SHA256(_stringJSON)
                Dim _hashBytesJSONString As String = ngApiUtilities.bytesToString(_hashBytesJSON)

                pESignature.pPutNalESignature = _signedJSON
                updSignPutNalHASH(pESignature.pPutNalID, pInData.pnDoc, _hashBytesJSONString, _stringJSON, 1)
                _retVal = insPutNalESignature(pESignature, pInData.pnDoc, pInData.signPos)

                insPutNalLog(pESignature.pPutNalID, pECert.employeeID, pInData.pnDoc, pInData.signPos, pESignature.ePublicKeyId)
            End If

        End If


        Return _retVal


    End Function














    'Private Function GetSelloFromPFX(ByVal cadenaOriginal As String) As String
    '    Dim sha1 As System.Security.Cryptography.SHA1CryptoServiceProvider = New System.Security.Cryptography.SHA1CryptoServiceProvider()
    '    Dim cert As System.Security.Cryptography.X509Certificates.X509Certificate2 = New System.Security.Cryptography.X509Certificates.X509Certificate2(Me.PFXArchivo, Me.PFXContrasena, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet)
    '    Dim rsaCryptoIPT As System.Security.Cryptography.RSACryptoServiceProvider = CType(cert.PrivateKey, System.Security.Cryptography.RSACryptoServiceProvider)
    '    Dim encoder As System.Text.UTF8Encoding = New System.Text.UTF8Encoding()
    '    Dim binData As Byte() = encoder.GetBytes(cadenaOriginal)
    '    Dim binSignature As Byte() = rsaCryptoIPT.SignData(binData, sha1)
    '    Dim sello As String = Convert.ToBase64String(binSignature)
    '    Return sello
    'End Function

    'Private Function convBytes2String(ByRef _hashBytes2 As Byte()) As String

    '    Dim SB As StringBuilder = New StringBuilder
    '    Dim HB As Byte
    '    For Each HB In _hashBytes2
    '        SB.Append(String.Format("{0:X2}", HB))
    '    Next

    '    Return SB.ToString()

    'End Function

    Private Function updSignPutNalHASH(ByVal pPutNalID As Integer, ByVal pPutNalDoc As Integer, ByVal pPutNalHASH As String, ByVal pPutNalEsignVal As String, ByVal pPutNalEsignType As Integer) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim _RowsAffected As Integer = -1

        strSQL = <![CDATA[
INSERT IGNORE INTO putninalog_hash (
pn_id, pn_doc, pn_hash, pn_esign_value, pn_esign_type
)
VALUES
  (
@pPutNalID, @pPutNalDoc, @pPutNalHASH, @pPutNalEsignVal, @pPutNalEsignType
  );
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL

            mycmd.Parameters.AddWithValue("@pPutNalID", pPutNalID)
            mycmd.Parameters.AddWithValue("@pPutNalDoc", pPutNalDoc)
            mycmd.Parameters.AddWithValue("@pPutNalHASH", pPutNalHASH)
            mycmd.Parameters.AddWithValue("@pPutNalEsignVal", pPutNalEsignVal)
            mycmd.Parameters.AddWithValue("@pPutNalEsignType", pPutNalEsignType)
            mycmd.Prepare()

            Try
                _RowsAffected = mycmd.ExecuteNonQuery()

                If _RowsAffected > 0 Then
                    Return True
                End If
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False

    End Function

    ' API eS2 status_to
    '    Public Function signStatusToPutNal(ByRef pESignature As NgESignature,
    '                               ByRef pNgPnPutniNalog As ngPnPutniNalog) As String


    '        Dim strSQL As String
    '        Dim _pn As ngPnPutniNalog = pNgPnPutniNalog

    '        Dim _ePublicKeyExist As Boolean = getPublicKey(pESignature)

    '        Dim _retVal As String = ""


    '        If pESignature.ePublicKey.Length > 0 Then

    '            strSQL = <![CDATA[ 
    'SELECT ps.`status_to` 
    'FROM `evd_potpisi_status` ps
    'INNER JOIN `evd_potpisi` p ON ps.`evd_potpisi_id` = p.`id`
    'WHERE 
    'p.`employee_id`= @employee_id
    'AND p.`aktivan` = -1
    'AND ps.`status_from`=@status_from;
    '    ]]>.Value
    '            Using myconnection As New MySqlConnection(ConnectionString)
    '                myconnection.Open()

    '                Dim mycmd As New MySqlCommand
    '                Dim dbRead As MySqlDataReader

    '                mycmd.Connection = myconnection


    '                mycmd.CommandText = strSQL
    '                mycmd.Prepare()
    '                mycmd.Parameters.Clear()
    '                mycmd.Parameters.AddWithValue("@employee_id", pESignature.pEmployeeID)
    '                mycmd.Parameters.AddWithValue("@status_from", pNgPnPutniNalog.status_naloga)


    '                dbRead = mycmd.ExecuteReader
    '                If dbRead.HasRows Then
    '                    While dbRead.Read()
    '                        _retVal = dbRead.GetString("status_to")
    '                    End While
    '                End If


    '            End Using

    '        End If


    '        Return _retVal

    '    End Function

    ' API eS2 status_to
    '    Public Function signStatusFromPutNal(ByRef pESignature As NgESignature,
    '                               ByRef pNgPnPutniNalog As ngPnPutniNalog) As String


    '        Dim strSQL As String
    '        Dim _pn As ngPnPutniNalog = pNgPnPutniNalog

    '        Dim _ePublicKeyExist As Boolean = getPublicKey(pESignature)

    '        Dim _retVal As String = ""


    '        If pESignature.ePublicKey.Length > 0 Then

    '            strSQL = <![CDATA[ 
    'SELECT ps.`status_from` 
    'FROM `evd_potpisi_status` ps
    'INNER JOIN `evd_potpisi` p ON ps.`evd_potpisi_id` = p.`id`
    'WHERE 
    'p.`employee_id`= @employee_id
    'AND p.`aktivan` = -1
    'AND ps.`status_to` = @status_to;
    '    ]]>.Value
    '            Using myconnection As New MySqlConnection(ConnectionString)
    '                myconnection.Open()

    '                Dim mycmd As New MySqlCommand
    '                Dim dbRead As MySqlDataReader

    '                mycmd.Connection = myconnection


    '                mycmd.CommandText = strSQL
    '                mycmd.Prepare()
    '                mycmd.Parameters.Clear()
    '                mycmd.Parameters.AddWithValue("@employee_id", pESignature.pEmployeeID)
    '                mycmd.Parameters.AddWithValue("@status_to", pNgPnPutniNalog.status_naloga)


    '                dbRead = mycmd.ExecuteReader
    '                If dbRead.HasRows Then
    '                    While dbRead.Read()
    '                        _retVal = dbRead.GetString("status_from")
    '                    End While
    '                End If


    '            End Using

    '        End If


    '        Return _retVal

    '    End Function

    ' Get PubliKey
    Private Function getPublicKey(ByRef _ngESignature As NgESignature) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable

        '
        ' Mora uvijek vratiti PUBLIC KEY, jer dokument može već biti potpisan
        ' iako je ključ neaktivan (ali je u vrijeme potpisivanja bio aktivan)
        '
        strSQL = <![CDATA[ 
SELECT ep.`potpis_publickey`, ep.id  FROM `evd_potpisi` ep
WHERE ep.`employee_id`=@employee_id
AND ep.`aktivan` = -1;
    ]]>.Value


        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlDataReader
            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employee_id", _ngESignature.pEmployeeID)
            mycmd.Prepare()

            _ngESignature.ePublicKey = ""
            _ngESignature.ePublicKeyId = -1

            dbRead = mycmd.ExecuteReader()
            While dbRead.Read()
                _ngESignature.ePublicKey = dbRead.GetString("potpis_publickey")
                _ngESignature.ePublicKeyId = dbRead.GetInt32("id")
            End While
            dbRead.Close()

        End Using

        If _ngESignature.ePublicKey.Length > 0 Then
            Return True
        Else
            Return False
        End If

    End Function

    ' API eS1
    Public Function insPutNalESignature(ByRef pESignature As NgESignature, ByVal pPnDoc As Integer, ByVal pSignPos As Integer) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As MySqlCommand = myconnection.CreateCommand()
            Dim _RowsAffected As Integer = -1



            Try
                mycmd.CommandText = <![CDATA[
REPLACE INTO putninalog_potpisi (pn_id, pn_esignature, pn_doc, pn_potpis, evd_potpisi_id, STATUS)
VALUES
  (
    @pPutNalID, @pESignatue, @pn_doc, @pn_potpis, @evd_potpisi_id, -1);
    ]]>.Value


                mycmd.Parameters.AddWithValue("@pPutNalID", pESignature.pPutNalID)
                mycmd.Parameters.AddWithValue("@pESignatue", pESignature.pPutNalESignature)
                mycmd.Parameters.AddWithValue("@evd_potpisi_id", pESignature.ePublicKeyId)      '''' pESignature.ePublicKeyId (isto što i ) pPotpisId 
                mycmd.Parameters.AddWithValue("@pn_doc", pPnDoc)
                mycmd.Parameters.AddWithValue("@pn_potpis", pSignPos)

                mycmd.Prepare()
                _RowsAffected = mycmd.ExecuteNonQuery()

                mycmd.CommandText = <![CDATA[
UPDATE putninalog_potpisi ep, putninalog_hash eh
SET ep.hash=eh.pn_hash
WHERE 
ep.pn_id = eh.pn_id 
AND ep.pn_doc = eh.pn_doc 
AND ep.evd_potpisi_id = @evd_potpisi_id
AND ep.pn_id = @pPutNalID;
    ]]>.Value

                mycmd.Prepare()
                _RowsAffected = mycmd.ExecuteNonQuery() + _RowsAffected


                Return If((_RowsAffected > 0), True, False)

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False

    End Function

    ' API eD1
    Public Function delPutNalESignature(ByRef pInData As ngPnPotpisByCert, ByVal pEvdPotpisiId As Integer) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        strSQL = <![CDATA[
DELETE FROM putninalog_potpisi 
WHERE 
pn_id = @pn_id
AND pn_doc = @pn_doc
AND pn_potpis = @pn_potpis
AND evd_potpisi_id = @evd_potpisi_id;

    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pn_id", pInData.pnID)
            mycmd.Parameters.AddWithValue("@pn_doc", pInData.pnDoc)
            mycmd.Parameters.AddWithValue("@pn_potpis", pInData.signPos)

            mycmd.Parameters.AddWithValue("@evd_potpisi_id", pEvdPotpisiId)

            mycmd.Prepare()

            Try
                Dim _RowsAffected = mycmd.ExecuteNonQuery()

                If _RowsAffected > 0 Then

                    insPutNalLog(pInData.pnID, pInData.employeeID, -pInData.pnDoc, -pInData.signPos, -pEvdPotpisiId)

                    Return True
                Else
                    Return False
                End If

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False

    End Function

    Public Function delPutNalPdfByCert(ByRef pInData As ngPnPotpisByCert, ByVal pEvdPotpisiId As Integer) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        strSQL = <![CDATA[
DELETE  
FROM
  `putninalog_doc_documents`
WHERE 
pn_id = @pn_id
AND pn_doc = @pn_doc
AND pn_potpis = @pn_potpis
AND evd_potpisi_id = @evd_potpisi_id;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pn_id", pInData.pnID)
            mycmd.Parameters.AddWithValue("@pn_doc", pInData.pnDoc)
            mycmd.Parameters.AddWithValue("@pn_potpis", pInData.signPos)

            mycmd.Parameters.AddWithValue("@evd_potpisi_id", pEvdPotpisiId)

            mycmd.Prepare()

            Try
                Dim _RowsAffected = mycmd.ExecuteNonQuery()

                If _RowsAffected > 0 Then
                    Return True
                Else
                    Return False
                End If

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False

    End Function

    Public Function delPutNalHash(ByRef pInData As ngPnPotpisByCert) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        strSQL = <![CDATA[
DELETE  
FROM
  `putninalog_hash`
WHERE 
pn_id = @pn_id
AND pn_doc = @pn_doc;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pn_id", pInData.pnID)
            mycmd.Parameters.AddWithValue("@pn_doc", pInData.pnDoc)
            mycmd.Prepare()

            Try
                Dim _RowsAffected = mycmd.ExecuteNonQuery()

                If _RowsAffected > 0 Then
                    Return True
                Else
                    Return False
                End If

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False

    End Function


    Public Function delESignature(ByVal pEmployeeID As Integer, ByVal _ESignId As Integer, ByRef pInData As ngPnPotpisByCert, ByVal pLastSignedFile As String) As Boolean

        Dim _ApiPutniNalog As New NgApiPnPutniNalog
        Dim _replyMess As Boolean = False

        ' Obriši iz tabela: 
        '                   putninalog_potpisi
        '                   putninalog_doc_documents    (kao i file)
        '
        ' ne dira se :      putninalog_hash
        '
        pInData.employeeID = pEmployeeID
        _replyMess = _ApiPutniNalog.delPutNalESignature(pInData, _ESignId)
        If (_replyMess = True) Then
            _replyMess = _ApiPutniNalog.delPutNalPdfByCert(pInData, _ESignId)
            File.Delete(pLastSignedFile)
        End If

        Dim _lastESignId As Integer = getLastSignedESignID(pInData.pnID, pInData.pnDoc)
        '
        ' Obrisani su svi potpisi, obriši i hash
        '
        If (_lastESignId = Nothing) AndAlso (_replyMess = True) Then
            _replyMess = _ApiPutniNalog.delPutNalHash(pInData)
        End If

        Return _replyMess

    End Function

    ' API esL1
    Public Function listESignature(ByRef pNgESignature As NgESignature) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        Dim _hasESignature As Boolean = False

        '
        ' Vraća PUBLIC KEY, samo ako je aktivan
        '
        strSQL = <![CDATA[
SELECT ep.`potpis_publickey`, ep.id  
FROM `evd_potpisi` ep
WHERE ep.`employee_id`=@employee_id 
AND ep.Aktivan <> 0;

    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlDataReader
            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employee_id", pNgESignature.pEmployeeID)
            mycmd.Prepare()

            pNgESignature.ePublicKey = ""
            pNgESignature.ePublicKeyId = -1

            dbRead = mycmd.ExecuteReader()
            If dbRead.HasRows Then
                _hasESignature = True
                While dbRead.Read()
                    pNgESignature.ePublicKey = dbRead.GetString("potpis_publickey")
                    pNgESignature.ePublicKeyId = dbRead.GetInt32("id")
                End While
                If pNgESignature.ePublicKey.Length = 0 Then _hasESignature = False
            End If
            dbRead.Close()
        End Using

        Return _hasESignature

    End Function

    ' API esC1
    Public Function insESignature(ByRef pNgESignature As NgESignature) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        '
        ' Sačuvaj PublicKey, jer ga listESignature poništi
        '
        Dim _publicKey As String = pNgESignature.ePublicKey
        Dim _hasESignature As Boolean = listESignature(pNgESignature)


        If _hasESignature Then
            Return False
        Else
            pNgESignature.ePublicKey = _publicKey


            strSQL = <![CDATA[
UPDATE `evd_potpisi` ep
SET ep.`potpis_publickey`= @potpis_publickey
WHERE ep.`employee_id`= @employee_id
AND aktivan = -1;
    ]]>.Value

            Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
                myconnection.Open()

                Dim mycmd As New MySqlCommand
                mycmd.Connection = myconnection

                mycmd.CommandText = strSQL


                mycmd.Parameters.Clear()
                mycmd.Parameters.AddWithValue("@employee_id", pNgESignature.pEmployeeID)
                mycmd.Parameters.AddWithValue("@potpis_publickey", pNgESignature.ePublicKey)
                mycmd.Prepare()

                Dim _rows As Integer = mycmd.ExecuteNonQuery()
                If _rows > 0 Then
                    _hasESignature = listESignature(pNgESignature)
                    Return True
                End If

            End Using

        End If

        Return False

    End Function


    ' API esD1
    Public Function delESignature(ByRef pNgESignature As NgESignature) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        '
        ' PUBLIC KEY setuje u neaktivan i dat_prestanka postavlja na CURRENT_TIMESTAMP
        '
        '   NE KORISTI SE!
        '
        strSQL = <![CDATA[
UPDATE
 evd_potpisi
SET
  dat_prestanka = CURRENT_TIMESTAMP, aktivan = 0
WHERE employee_id = @employee_id
AND  potpis_publickey = @potpis_publickey;
    ]]>.Value



        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As MySqlCommand = myconnection.CreateCommand()


            mycmd.CommandText = <![CDATA[
UPDATE
 evd_potpisi
SET
  dat_prestanka = CURRENT_TIMESTAMP, aktivan = 0
WHERE employee_id = @employee_id
AND aktivan = -1;
    ]]>.Value


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employee_id", pNgESignature.pEmployeeID)
            mycmd.Parameters.AddWithValue("@potpis_publickey", pNgESignature.ePublicKey)
            mycmd.Prepare()

            Dim _rows As Integer = mycmd.ExecuteNonQuery()
            If _rows > 0 Then Return True

        End Using

        Return False

    End Function

    ' API esRD
    Public Function readyESignature(ByVal pEmployeeID As Integer, Optional ByVal pAktivan As Integer = -1) As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        Dim _NgESignature As New NgESignature
        _NgESignature.pEmployeeID = pEmployeeID

        '
        ' Provjera status EPotpisa u sql tabeli
        '
        Dim _reply = listPublicKey(_NgESignature)

        If _reply.ePublicKeyStatus = "no_signature" OrElse _reply.ePublicKeyStatus = "deactivated" Then

            '
            ' ...
            '
            strSQL = <![CDATA[
INSERT INTO evd_potpisi ( employee_id, aktivan)
VALUES
  (
    @employee_id, @aktivan
  );

    ]]>.Value


            Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
                myconnection.Open()

                Dim mycmd As New MySqlCommand

                mycmd.Connection = myconnection

                mycmd.CommandText = strSQL


                mycmd.Parameters.Clear()
                mycmd.Parameters.AddWithValue("@employee_id", pEmployeeID)
                mycmd.Parameters.AddWithValue("@aktivan", pAktivan)
                mycmd.Prepare()

                Dim _rows As Integer = mycmd.ExecuteNonQuery()
                If _rows > 0 Then Return True

            End Using


        End If


        Return False

    End Function

    ' API esPK
    Public Function listPublicKey(ByRef pNgESignature As NgESignature) As NgESignature

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String


        '
        ' Vraća PUBLIC KEY, samo ako je aktivan
        '
        strSQL = <![CDATA[
SELECT ep.`potpis_publickey`, ep.id, ep.aktivan  
FROM `evd_potpisi` ep
WHERE ep.`employee_id`=@employee_id AND  ep.aktivan = -1;
    ]]>.Value

        '   Ako je pNgESignature.PublicKey prazan - Sql upit prema EmployeeId, vraća:
        '       - no_signature, HasRows je 0, nema redova u tabelu evd_potpisi
        '       - create_keys, HasRows je 1, postoji red u tabeli evd_potpisi - ali je potpis_publickey prazan
        '       - activated, HasRows je 1, postoji red u tabeli evd_potpisi - a potpis_publickey je već upisan
        '


        '
        '   Ako je pNgESignature.PublicKey poslan - Sql upit prema EmployeeId i PublicKey, vraća:
        '       - activated, HasRows je 1, postoji red u tabeli evd_potpisi - a potpis_publickey je već upisan
        '       - deactivated, HasRows je 1, postoji red u tabeli evd_potpisi - a potpis_publickey je već upisan, ali je Aktivan = 0
        '       - TODO: waiting_activation (-2), HasRows je 1, postoji red u tabeli evd_potpisi - a potpis_publickey je već upisan ali je Aktivan = -2
        '       - no_signature, HasRows je 0, ne postoji red u tabeli evd_potpisi - sa parametrima EmployeeId i PublicKey
        '


        Dim _NgESignature As New NgESignature
        Dim _NgESigAktivan As Integer = 0


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlDataReader
            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employee_id", pNgESignature.pEmployeeID)
            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader()
            If dbRead.HasRows Then

                While dbRead.Read()
                    _NgESignature.ePublicKey = dbRead.GetString("potpis_publickey")
                    _NgESignature.ePublicKeyId = dbRead.GetInt32("id")
                    _NgESigAktivan = dbRead.GetInt16("aktivan")
                End While

                Select Case _NgESigAktivan
                    Case -1                 ' aktivan
                        '----------------------------
                        Select Case _NgESignature.ePublicKey.Length

                            Case 0          ' nema PublicKey u bazi
                                '----------------------------------
                                '           ' nema PublicKey u API pozivu
                                If (pNgESignature.ePublicKey.Length = 0) Then _NgESignature.ePublicKeyStatus = "create_keys"
                                '           ' ima PublicKey u API pozivu
                                '           ' upisati PublicKey u bazu
                                If (pNgESignature.ePublicKey.Length > 0) Then
                                    If insESignature(pNgESignature) Then
                                        _NgESignature.ePublicKeyStatus = "activated"
                                        _NgESignature.ePublicKey = pNgESignature.ePublicKey
                                    Else
                                        _NgESignature.ePublicKeyStatus = "failed_activation"
                                    End If
                                End If

                            Case > 0        ' ima PublicKey u bazi
                                '---------------------------------
                                '           ' ima PublicKey u API pozivu - ISTI je kao u bazi
                                If (_NgESignature.ePublicKey = pNgESignature.ePublicKey) Then _NgESignature.ePublicKeyStatus = "activated"
                                '           ' ima PublicKey u API pozivu - NIJE ISTI kao u bazi
                                If (_NgESignature.ePublicKey <> pNgESignature.ePublicKey) Then _NgESignature.ePublicKeyStatus = "failed_signature"

                        End Select

                    Case -2

                    Case 0                  ' neaktivan
                        '------------------------------
                        Select Case _NgESignature.ePublicKey.Length

                            Case 0          ' nema PublicKey u bazi
                                '----------------------------------
                                '           ' nema PublicKey u API pozivu
                                _NgESignature.ePublicKeyStatus = "no_signature"

                            Case > 0        ' ima PublicKey u bazi
                                '---------------------------------
                                '           ' ima PublicKey u API pozivu - isti je kao u bazi
                                If (_NgESignature.ePublicKey = pNgESignature.ePublicKey) Then _NgESignature.ePublicKeyStatus = "deactivated"

                        End Select

                    Case Else

                End Select

            Else
                _NgESignature.ePublicKeyStatus = "no_signature"
            End If

            dbRead.Close()
        End Using

        _NgESignature.pEmployeeID = pNgESignature.pEmployeeID

        Return _NgESignature

    End Function

    ' API esAK
    Public Function authorizeESignature(ByVal pEvdPotpisiID As Integer, Optional ByVal pDocument As Integer = 1,
                                        Optional ByVal pStatusFrom As String = "otvoren",
                                         Optional ByVal pStatusTo As String = "odobren") As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        '
        ' PUBLIC KEY setuje u neaktivan i dat_prestanka postavlja na CURRENT_TIMESTAMP
        '
        strSQL = <![CDATA[
INSERT INTO evd_potpisi_status (
  evd_potpisi_id, document, status_from, status_to
)
VALUES
  (
    @evd_potpisi_id, @document, @status_from, @status_to
  );
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@evd_potpisi_id", pEvdPotpisiID)
            mycmd.Parameters.AddWithValue("@document", pDocument)
            mycmd.Parameters.AddWithValue("@status_from", pStatusFrom)
            mycmd.Parameters.AddWithValue("@status_to", pStatusTo)
            mycmd.Prepare()

            Dim _rows As Integer = mycmd.ExecuteNonQuery()
            If _rows > 0 Then Return True

        End Using

        Return False

    End Function

    ' API esAD
    Public Function delAuthorizeESignature(ByVal pEvdPotpisiID As Integer, Optional ByVal pDocument As Integer = 1,
                                        Optional ByVal pStatusFrom As String = "otvoren") As Boolean

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        '
        ' PUBLIC KEY setuje u neaktivan i dat_prestanka postavlja na CURRENT_TIMESTAMP
        '
        strSQL = <![CDATA[
            DELETE
FROM
 evd_potpisi_status
WHERE
    evd_potpisi_id = @evd_potpisi_id
    AND document = @document
    AND status_from = @status_from;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@evd_potpisi_id", pEvdPotpisiID)
            mycmd.Parameters.AddWithValue("@document", pDocument)
            mycmd.Parameters.AddWithValue("@status_from", pStatusFrom)
            mycmd.Prepare()

            Dim _rows As Integer = mycmd.ExecuteNonQuery()
            If _rows > 0 Then Return True

        End Using

        Return False

    End Function

    ' API esUL1
    Public Function getEsignEmployeeList(ByVal pLstOrg As String, ByVal pLstPG As String, ByVal pTmpTableName As String,
                                         ByVal pEmployeeId As Integer) As List(Of NgUser)

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL, strSQLStart, strSQLOrg, strSQLPG, strSQLEnd As String
        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        strSQL = <![CDATA[ 

        ]]>.Value

        strSQLStart = <![CDATA[ 
    DROP TABLE IF EXISTS %__tmp_epotpisi__%;

    CREATE TEMPORARY TABLE  IF NOT EXISTS `%__tmp_epotpisi__%` (
      `EmployeeID` INT(11) NOT NULL,
      `LastName` VARCHAR(50) CHARACTER SET cp1250 DEFAULT NULL,
      `FirstName` VARCHAR(50) CHARACTER SET cp1250 DEFAULT NULL,
      `Naziv` VARCHAR(255) CHARACTER SET cp1250 DEFAULT NULL,
      `Naziv2` VARCHAR(255) CHARACTER SET cp1250 DEFAULT NULL,
      `OrgJedSifra` VARCHAR(255) CHARACTER SET cp1250 DEFAULT NULL,
      `Email_pos` VARCHAR(45) CHARACTER SET utf8 NOT NULL DEFAULT '''',
      `user_status` INT(1) NOT NULL DEFAULT '0',
      `mark_status` INT(1) NOT NULL DEFAULT '0',
      PRIMARY KEY (`EmployeeID`)
    ) ENGINE=INNODB DEFAULT CHARSET=latin1;

        ]]>.Value

        strSQLOrg = <![CDATA[ 

    INSERT IGNORE INTO %__tmp_epotpisi__%
    SELECT e.EmployeeID,  e.LastName, e.FirstName, 
    o.Naziv AS OrgJedNaziv, 
    o.Sifra,
    o.Sifra,
    IFNULL(ep.`custom2`, '') AS Email_pos, 
    0 AS user_status, 0 AS mark_status
    FROM `evd_employees` e 
    INNER JOIN evd_employees_lpd el ON e.`EmployeeID`=el.`EmployeeID`
    INNER JOIN `sfr_organizacija` o ON e.`DepartmentUP`=o.`id` 
    LEFT JOIN `evd_employees_pim` ep ON el.`EmployeeID` = ep.`EmployeeID`
    WHERE 
    o.`Sifra` IN(%__ListOrgJed__%)
    AND Aktivan <> 0 
    ORDER BY e.LastName;
        ]]>.Value

        strSQLPG = <![CDATA[ 
    INSERT IGNORE INTO %__tmp_epotpisi__%
    SELECT e.EmployeeID,  e.LastName, e.FirstName, 
    o.Naziv AS OrgJedNaziv, 
    o.Sifra,
    o.Sifra,
    IFNULL(ep.emp_work_email,'') AS Email, 
    0 AS user_status, 0 AS mark_status    
    FROM `evd_employees` e 
    INNER JOIN evd_employees_lpd el ON e.`EmployeeID`=el.`EmployeeID`
    INNER JOIN `sfr_organizacija` o ON e.`DepartmentUP`=o.`id`
     LEFT JOIN `evd_employees_pim` ep ON el.`EmployeeID` = ep.`EmployeeID`    
    WHERE
    e.EmployeeID IN(%__ListPGJed__%) AND Aktivan <> 0 ORDER BY e.LastName;
        ]]>.Value

        strSQLEnd = <![CDATA[ 
    UPDATE %__tmp_epotpisi__% t, `evd_employees` e, `my_aspnet_users_to_employees` m 
    SET t.user_status=-1
    WHERE e.`EmployeeID`=m.`employees_id`
    AND t.EmployeeID=e.EmployeeID;

    DELETE
    FROM
       %__tmp_epotpisi__%
    WHERE user_status = 0;

    UPDATE %__tmp_epotpisi__% t
    SET t.user_status=0;
    
    UPDATE %__tmp_epotpisi__% t
    SET t.mark_status=0;
        
    UPDATE %__tmp_epotpisi__% t, evd_potpisi p
    SET t.user_status=-1
    WHERE t.EmployeeID=p.employee_id
    AND p.aktivan <> 0;
    
    UPDATE %__tmp_epotpisi__% t, evd_potpisi p, evd_potpisi_status ps
    SET t.mark_status=-1
    WHERE t.EmployeeID=p.employee_id
    AND p.aktivan <> 0
    AND ps.evd_potpisi_id=p.id
    AND ps.document = 1;


    UPDATE %__tmp_epotpisi__% t, `sfr_organizacija` o
    SET 
    t.Naziv = CONCAT(t.Naziv,' (',t.OrgJedSifra,')'),
    t.Naziv2 =o.`Naziv`
    WHERE SUBSTR(t.OrgJedSifra, 1,3)=o.`Sifra`;

    SELECT 
      `EmployeeID`,
      `LastName`,
      `FirstName`,
      `Naziv` AS OrgJedNaziv,
      `Naziv2` AS OrgJedNaziv2,
      OrgJedSifra,
      `Email_pos` AS Email,
      `user_status` AS Epotpis,
      `mark_status` AS Epotpisnik     
    FROM  %__tmp_epotpisi__% 
    ORDER BY OrgJedSifra, LastName

        ]]>.Value

        Dim rawJson As String = ""

        If pLstOrg.Length > 0 AndAlso pLstPG.Length > 0 Then
            strSQL = strSQLStart + strSQLOrg + strSQLPG + strSQLEnd
        ElseIf pLstOrg.Length > 0 AndAlso pLstPG.Length = 0 Then
            strSQL = strSQLStart + strSQLOrg + strSQLEnd
        ElseIf pLstOrg.Length = 0 AndAlso pLstPG.Length > 0 Then
            strSQL = strSQLStart + strSQLPG + strSQLEnd
        ElseIf pLstOrg.Length = 0 AndAlso pLstPG.Length = 0 Then
            strSQL = strSQLStart + strSQLEnd
        End If

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            strSQL = strSQL.Replace("%__tmp_epotpisi__%", pTmpTableName)
            strSQL = strSQL.Replace("%__ListOrgJed__%", pLstOrg)
            strSQL = strSQL.Replace("%__ListPGJed__%", pLstPG)
            strSQL = strSQL.Replace("%__EmployeeId__%", pEmployeeId)

            mycmd.CommandText = strSQL


            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)
        End Using


        Return JsonConvert.DeserializeObject(Of List(Of NgUser))(GetJson(DtList))

    End Function

    Private Function GetJson(ByVal dt As DataTable) As String
        Dim serializer As New System.Web.Script.Serialization.JavaScriptSerializer()
        Dim rows As New List(Of Dictionary(Of String, Object))()
        Dim row As Dictionary(Of String, Object) = Nothing
        For Each dr As DataRow In dt.Rows
            row = New Dictionary(Of String, Object)()
            For Each dc As DataColumn In dt.Columns
                'If dc.ColumnName.Trim() = "TAGNAME" Then
                row.Add(dc.ColumnName.Trim(), dr(dc))
                'End If
            Next
            rows.Add(row)
        Next
        Return serializer.Serialize(rows)
    End Function

    Public Shared Function getDate(ByVal pDateString As String) As Date

        Dim format As String
        Dim result As Date
        Dim provider As CultureInfo = CultureInfo.InvariantCulture

        format = "yyyy-MM-dd"
        Try
            result = Date.ParseExact(pDateString, format, provider)
            Return result
        Catch e As FormatException

        End Try

        Return Nothing

    End Function

End Class
