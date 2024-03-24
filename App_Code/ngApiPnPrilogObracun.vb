Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json

Public Class ngApiPnPrilogObracun
    Inherits ngApiPnPrilog

    ' API Sql.Select
    '    Public Overloads Function selPrilog(ByVal pId As Integer, ByVal pPnId As Integer) As List(Of ngPnPrilog)

    '        Dim strSQL As String = <![CDATA[ 
    'SELECT
    '  pp.`id`,
    '  pp.`pn_id`,
    '  pp.`redbr`,
    '  pp.`opis`,
    '  pp.`iznos_val`,
    '  pp.`iznos`,
    '  pp.`kategorija`,
    '  pp.pn_obracun_id,
    '  CONCAT(po.`pn_oblast`,'.',po.`order_id`) AS obracun,
    '  pp.`odobren`,
    '  pp.`napomena`,
    '  DATE_FORMAT(pp.dummytimestamp, '%Y-%m-%dT%H:%i:%sZ') AS `timestamp`
    'FROM
    '  `putninalog_prilozi` pp LEFT JOIN `putninalog_obracun` po ON pp.`pn_obracun_id`=po.`id`
    'WHERE 
    'pp.pn_id=@pn_id
    'AND 
    'pp.`id`=@id;
    '    ]]>.Value

    '        Return sqlFill(strSQL, pId, pPnId)

    '    End Function

    ' API Sql.Insert
    Public Function insPrilogObracun(ByVal pId As Integer, ByVal pPnId As Integer) As Boolean

        Dim strSQL As String = <![CDATA[

INSERT IGNORE INTO `putninalog_obracun` (
  `pn_id`,
  `pn_oblast`,
  `ukupan_iznos`,
  `obracun_ostalih_troskova`,
   razred,
  `order_id`
)
SELECT 
@pn_id,
CASE pp.`kategorija` 
WHEN 1 THEN 2
WHEN 2 THEN 3
WHEN 3 THEN 2
WHEN 4 THEN 2
WHEN 5 THEN 4
END  rb,
pp.`iznos`,
pp.`opis`,
pp.`opis`,
1
FROM `putninalog_prilozi` pp
WHERE pp.`id`= @id
AND (pp.`kategorija` IS NOT NULL
AND pp.`kategorija`<>0)
AND pp.`odobren` <> 0;
    ]]>.Value

        Dim pPnprilog = New ngPnPrilog With {.id = pId, .pn_id = pPnId}

        Dim _retVal = sqlExecute(strSQL, pId, pPnprilog)

        If _retVal = True And LastInsertedId > 0 Then
            ' Update pn_prilog sa id pn_obracun radi reference

            strSQL = <![CDATA[
UPDATE `putninalog_prilozi` pp
SET pp.`pn_obracun_id` = @pn_obracun_id
WHERE pp.`id`= @id;
    ]]>.Value

            pPnprilog.pn_obracun_id = LastInsertedId

            Return sqlExecute(strSQL, pId, pPnprilog)
        End If

        Return False

    End Function

    ' API Sql.Update
    Public Function updPrilogObracun(ByVal pId As Integer, ByVal pPnId As Integer) As Boolean

        Dim strSQL As String = <![CDATA[
UPDATE
  `putninalog_obracun` po, `putninalog_prilozi` pp
SET
  po.`ukupan_iznos` = pp.`iznos`,
  po.`obracun_ostalih_troskova` = pp.`opis`,
  po.`razred` = pp.`opis`
WHERE po.`id` = pp.`pn_obracun_id`
AND pp.`id`= @id;
    ]]>.Value

        Dim pPnprilog = New ngPnPrilog With {.id = pId, .pn_id = pPnId}

        Return sqlExecute(strSQL, pId, pPnprilog)

    End Function

    ' API Sql.Delete
    Public Function delPrilogObracun(ByVal pId As Integer, ByVal pPnId As Integer) As Boolean

        Dim strSQL As String = <![CDATA[
DELETE po.*
FROM
  `putninalog_obracun` po, `putninalog_prilozi` pp
WHERE po.`id` = pp.`pn_obracun_id`
AND po.`pn_id` = pp.`pn_id`
AND pp.`id` = @id
AND po.`pn_id` = @pn_id;;
    ]]>.Value

        Dim pPnprilog = New ngPnPrilog With {.id = pId, .pn_id = pPnId}

        If (sqlExecute(strSQL, pId, pPnprilog) = True) Then
            ' Update pn_prilog sa id pn_obracun radi reference

            strSQL = <![CDATA[
UPDATE `putninalog_prilozi` pp
SET pp.`pn_obracun_id` = NULL
WHERE pp.`id`= @id
AND pp.`pn_id` = @pn_id;;
    ]]>.Value


            Return sqlExecute(strSQL, pId, pPnprilog)
        End If

        Return False

    End Function

    ' API Sql.Update Obracun
    Public Function updIzvObracun(ByVal pPnId As Integer) As Boolean

        Dim strSQL As String = <![CDATA[
UPDATE `putninalog_izvjestaj_dod` pid
SET pid.`ostali_troskovi` = 0
WHERE pid.`pn_id` = @pn_id
AND pid.`pn_oblast` = 4;

UPDATE `putninalog_izvjestaj_dod` pid,
(SELECT pp.`kategorija`, COUNT(pp.`id`) AS num
FROM `putninalog_prilozi` pp
WHERE pp.`pn_id` = @pn_id
GROUP BY pp.`kategorija`) c
SET pid.`ostali_troskovi` = c.num
WHERE pid.`pn_id` = @pn_id
AND c.kategorija = pid.`order_id`;
    ]]>.Value

        Dim pPnprilog = New ngPnPrilog With {.id = -1, .pn_id = pPnId}

        Return sqlExecute(strSQL, pPnId, pPnprilog)

    End Function
End Class
