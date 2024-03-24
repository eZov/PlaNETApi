Imports System.Data
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json

''' <summary>
''' Objekat koji predstavlja evidencije prisustva. Evidencije predstavljaju jedan kalendarski mjesec.
''' </summary>
Public Class ngApiPnPrilog

    Public Sub New()

    End Sub

    Public Property LastInsertedId As Long

    ' API Sql.Insert
    Public Function insPrilog(ByVal pID As Integer, ByRef pPnPrilog As ngPnPrilog) As Boolean

        Dim strSQL As String = <![CDATA[
INSERT INTO `putninalog_prilozi` (
  `pn_id`,
  `redbr`,
  `opis`,
  `iznos_val`,
  `iznos`,
  `kategorija`,
  `odobren`,
  `napomena`
)
VALUES
  (
    @pn_id,
    @redbr,
    @opis,
    @iznos_val,
    @iznos,
    @kategorija,
    @odobren,
    @napomena
  );
    ]]>.Value


        Return sqlExecute(strSQL, pID, pPnPrilog)

    End Function

    ' API Sql.Update
    Public Function updPrilog(ByVal pID As Integer, ByRef pPnPrilog As ngPnPrilog) As Boolean

        Dim strSQL As String = <![CDATA[
UPDATE
  `putninalog_prilozi`
SET
  `id` = @id,
  `pn_id` = @pn_id,
  `redbr` = @redbr,
  `opis` = @opis,
  `iznos_val` = @iznos_val,
  `iznos` = @iznos,
  `kategorija` = @kategorija,
  `odobren` = @odobren,
  `napomena` = @napomena
WHERE `id` = @id AND pn_id = @pn_id;
    ]]>.Value


        Return sqlExecute(strSQL, pID, pPnPrilog)

    End Function

    ' API Sql.Delete
    Public Function delPrilog(ByVal pID As Integer, ByRef pPnPrilog As ngPnPrilog) As Boolean

        Dim strSQL As String = <![CDATA[
DELETE
FROM
  `putninalog_prilozi`
WHERE `id` = @id AND pn_id = @pn_id;
    ]]>.Value

        Return sqlExecute(strSQL, pID, pPnPrilog)

    End Function

    ' API Sql.Select
    Public Function selPrilog(ByVal pId As Integer) As List(Of ngPnPrilog)

        Dim strSQL As String = <![CDATA[ 
SELECT
  pp.`id`,
  pp.`pn_id`,
  pp.`redbr`,
  pp.`opis`,
  pp.`iznos_val`,
  pp.`iznos`,
  IFNULL(pp.`kategorija`,0) AS kategorija,
  pp.pn_obracun_id,
  CONCAT(po.`pn_oblast`,'.',po.`order_id`) AS obracun,
  pp.`odobren`,
  pp.`napomena`,
  DATE_FORMAT(pp.dummytimestamp, '%Y-%m-%dT%H:%i:%sZ') AS `timestamp`
FROM
  `putninalog_prilozi` pp LEFT JOIN `putninalog_obracun` po ON pp.`pn_obracun_id`=po.`id`
WHERE pp.pn_id=@pn_id
ORDER BY redbr;
    ]]>.Value

        Return sqlFill(strSQL, pId)

    End Function

    Private Sub sqlParameters(ByRef pCmd As MySqlCommand, ByVal pID As Integer, ByRef pPnPrilog As ngPnPrilog)

        pCmd.Parameters.AddWithValue("@id", pID)
        pCmd.Parameters.AddWithValue("@pn_id", pPnPrilog.pn_id)

        pCmd.Parameters.AddWithValue("@redbr", pPnPrilog.redbr)
        pCmd.Parameters.AddWithValue("@opis", pPnPrilog.opis)
        pCmd.Parameters.AddWithValue("@iznos_val", pPnPrilog.iznos_val)
        pCmd.Parameters.AddWithValue("@iznos", pPnPrilog.iznos)
        pCmd.Parameters.AddWithValue("@kategorija", pPnPrilog.kategorija)
        pCmd.Parameters.AddWithValue("@pn_obracun_id", pPnPrilog.pn_obracun_id)
        pCmd.Parameters.AddWithValue("@odobren", pPnPrilog.odobren)
        pCmd.Parameters.AddWithValue("@napomena", pPnPrilog.napomena)

    End Sub

    Protected Function sqlExecute(pCmdText As String, ByVal pID As Integer, ByRef pPnPrilog As ngPnPrilog) As Boolean

        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = pCmdText


            mycmd.Parameters.Clear()
            sqlParameters(mycmd, pID, pPnPrilog)
            mycmd.Prepare()

            Try
                If (mycmd.ExecuteNonQuery() = 1) Then
                    LastInsertedId = mycmd.LastInsertedId
                    Return True
                End If

            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False

    End Function

    Protected Function sqlFill(pCmdText As String, ByVal pID As Integer) As List(Of ngPnPrilog)
        Dim myda As MySqlDataAdapter
        Dim DtList As New DataTable

        Using myconnection As New MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = pCmdText


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@pn_id", pID)
            mycmd.Prepare()

            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)

            'If DtList.Rows.Count > 0 Then
            Dim _serDtList = ngApiGeneral.GetJson(DtList)
                Return JsonConvert.DeserializeObject(Of List(Of ngPnPrilog))(_serDtList)
            'Else
            '    Dim _serDtList = ngApiGeneral.GetJson(DtList)
            '    Return JsonConvert.DeserializeObject(Of List(Of ngPnPrilog))(_serDtList)
            'End If


        End Using

        'Return Nothing

    End Function
End Class
