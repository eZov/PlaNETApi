Imports System.Data
Imports System.Reflection
Imports System.Threading
Imports System.IO
Imports System.Net
Imports System.Web.Http
Imports Newtonsoft.Json
Imports Syncfusion.EJ2.PdfViewer
Imports System.Net.Http
Imports Syncfusion.Linq
Imports System.Drawing
Imports Syncfusion.Pdf.Parsing
Imports System.Net.Mail
Imports System.Web
Imports System.Collections.Generic

Public Class JwtputnalController
    Inherits ApiController

    Public domainName As String = ""
    Public domainConnectionString As String = ""

    ' GET api/<controller>
    Public Function GetValues() As IEnumerable(Of String)
        Return New String() {"value1", "value2"}
    End Function

    ' GET api/<controller>/5
    Public Function GetValue(ByVal id As Integer) As String
        Return "value"
    End Function

    ' POST api/<controller>
    Public Sub PostValue(<FromBody()> ByVal value As String)

    End Sub

    ' PUT api/<controller>/5
    Public Sub PutValue(ByVal id As Integer, <FromBody()> ByVal value As String)

    End Sub

    ' DELETE api/<controller>/5
    Public Sub DeleteValue(ByVal id As Integer)

    End Sub


    <HttpGet>
    <Authorize>
    Public Function SpNalogs(ByVal pEmpID As Integer) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '

        If pEmpID = -1 Then pEmpID = _ApiSession.EmployeeID

        '
        ' Pretvori aktivan, odobren... u 'aktivan','odobren'...
        '
        Dim _parStatus As String = "otvoren, odobren"
        _parStatus = _parStatus.Replace(" ", "")
        Dim _lstStatus As List(Of String) = _parStatus.Split(",").ToList()
        Dim _lstStatusFormat As New List(Of String)

        For Each el As String In _lstStatus
            el = String.Format("{0}{1}{2}", "'", el, "'")
            _lstStatusFormat.Add(el)
        Next

        _parStatus = String.Join(",", _lstStatusFormat.ToArray())

        Dim _ApiPutniNalog As New NgApiPnPutniNalog(pEmpID, _parStatus)
        Dim unserializedContent = _ApiPutniNalog.PnPutniNalogList

        Return Json(unserializedContent)

    End Function

    ' API L1
    <HttpGet>
    <Authorize>
    Public Function PutNals() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        'Dim _parPotpis As Boolean = False
        Dim _parEmpID As Integer = -1
        Dim _parList As String = ""
        Dim _parDays As Integer = 0
        Dim _parStatus As String = ""

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "pEmpID"
                    If Not (Integer.TryParse(keypair.Value, _parEmpID) = True) Then
                        _parEmpID = 0
                    End If
                Case "list"
                    _parList = keypair.Value
                Case "days"
                    Integer.TryParse(keypair.Value, _parDays)
                Case "status"
                    _parStatus = keypair.Value
                    '
                    ' Pretvori aktivan, odobren... u 'aktivan','odobren'...
                    '
                    _parStatus = _parStatus.Replace(" ", "")
                    Dim _lstStatus As List(Of String) = _parStatus.Split(",").ToList()
                    Dim _lstStatusFormat As New List(Of String)

                    For Each el As String In _lstStatus
                        el = String.Format("{0}{1}{2}", "'", el, "'")
                        _lstStatusFormat.Add(el)
                    Next

                    _parStatus = String.Join(",", _lstStatusFormat.ToArray())
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _ApiPutniNalog As New NgApiPnPutniNalog()

        Select Case _ApiSession.Role

            Case "uposlenik"

                _parEmpID = _ApiSession.EmployeeID
                _ApiPutniNalog.getPnPutniNalog(_parEmpID, _parStatus, _parDays)

            Case Else
                If _parEmpID = -1 Then
                    _parEmpID = _ApiSession.EmployeeID
                    _ApiPutniNalog.getPnPutniNalog(_parEmpID, _parStatus, _parDays)
                Else
                    _ApiPutniNalog.getPnPutniNalog(_parEmpID, _parStatus, _parDays)
                End If



        End Select



        Dim unserializedContent = _ApiPutniNalog.PnPutniNalogList

        Return Json(unserializedContent)

    End Function

    ' API L2
    <HttpGet>
    <Authorize>
    Public Function PutNalsByOrgRole() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        'Dim _parPotpis As Boolean = False
        Dim _parList As String = ""
        Dim _createdby As Integer = -1
        Dim _parExtEmployees As String = ""
        Dim _parDays As Integer = 0
        Dim _parStatus As String = ""
        Dim _parVrsta As String = ""
        Dim _pn_doc As Integer = -1

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "list"
                    _parList = keypair.Value
                Case "createdby"
                    Integer.TryParse(keypair.Value, _createdby)
                Case "ext"
                    _parExtEmployees = keypair.Value
                Case "days"
                    Integer.TryParse(keypair.Value, _parDays)
                Case "status"
                    _parStatus = keypair.Value
                    '
                    ' Pretvori aktivan, odobren... u 'aktivan','odobren'...
                    '
                    _parStatus = _parStatus.Replace(" ", "")
                    Dim _lstStatus As List(Of String) = _parStatus.Split(",").ToList()
                    Dim _lstStatusFormat As New List(Of String)

                    For Each el As String In _lstStatus
                        el = String.Format("{0}{1}{2}", "'", el, "'")
                        _lstStatusFormat.Add(el)
                    Next

                    _parStatus = String.Join(",", _lstStatusFormat.ToArray())
                Case "vrsta"
                    _parVrsta = keypair.Value
                Case "pndoc"
                    Integer.TryParse(keypair.Value, _pn_doc)
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _ngApiOrganizacija As New NgApiOrganizacija
        Dim _ApiPutniNalog As New NgApiPnPutniNalog()

        Select Case _parList
            Case "byorg"


                ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                '
                Dim _roleOrgIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email)
                '
                ' Uzmi rekurzivno sve org id u hijerarhiji organizacije
                _ngApiOrganizacija.GetOrgSifra(_roleOrgIds, 5, 1)

                Dim _orgIds As String = String.Join(",", _ngApiOrganizacija.lstOrgSifras)
                Dim _tmpTable As String = "tmp_" + _ApiSession.EmployeeID.ToString + "_putnalozi"

                'Dim _ApiPutniNalog As New NgPnApiPutniNalog()
                If _orgIds.Length > 0 Then
                    _ApiPutniNalog.getPnOrgPutniNalog(_orgIds, _tmpTable, _parStatus, _ApiSession.EmployeeID, _parDays, _createdby, _parExtEmployees, _parVrsta)
                End If


            Case "bypg"

                ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                '
                Dim _rolePGIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email, True)
                '
                _ngApiOrganizacija.GetPGEmpIds(_rolePGIds)
                Dim _PGIds As String = String.Join(",", _ngApiOrganizacija.lstEmpIds)
                Dim _tmpTable As String = "tmp_" + _ApiSession.EmployeeID.ToString + "_putnalozi"

                'Dim _ApiPutniNalog As New NgPnApiPutniNalog()
                If _PGIds.Length > 0 Then
                    _ApiPutniNalog.getPnPGPutniNalog(_PGIds, _tmpTable, _parStatus, _ApiSession.EmployeeID, _parDays, _createdby, _parExtEmployees, _parVrsta, _pn_doc)
                End If

            Case "byorgpg"


                ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                '
                Dim _roleOrgIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email)
                '
                ' Uzmi rekurzivno sve org id u hijerarhiji organizacije
                _ngApiOrganizacija.GetOrgSifra(_roleOrgIds, 5, 1)
                Dim _orgIds As String = String.Join(",", _ngApiOrganizacija.lstOrgSifras)

                Dim _rolePGIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email, True)
                _ngApiOrganizacija.GetPGEmpIds(_rolePGIds)
                Dim _PGIds As String = String.Join(",", _ngApiOrganizacija.lstEmpIds)

                Dim _tmpTable As String = "tmp_" + _ApiSession.EmployeeID.ToString + "_putnalozi"

                'Dim _ApiPutniNalog As New NgPnApiPutniNalog()
                If _orgIds.Length > 0 Or _PGIds.Length > 0 Then
                    _ApiPutniNalog.getPnOrgPGPutniNalog(_orgIds, _PGIds, _tmpTable, _parStatus, _ApiSession.EmployeeID, _parDays, _createdby, _parExtEmployees, _parVrsta, _pn_doc)
                End If

                Dim _PGExt As String = _ngApiOrganizacija.GetPGExterni(_rolePGIds)
                If _PGExt.Length > 0 Then
                    Dim _idx = _PGExt.LastIndexOf("OR")
                    Dim _PGExtSql = _PGExt.Remove(_idx, 2)
                    _ApiPutniNalog.getPnPGExtPutniNalog(_PGExtSql, _tmpTable, _parStatus, _ApiSession.EmployeeID, _parDays, _createdby, _parExtEmployees, _parVrsta, _pn_doc)
                End If


        End Select






        Dim unserializedContent = _ApiPutniNalog.PnPutniNalogList

        Return Json(unserializedContent)

    End Function


    ' API DevListOrg
    <HttpGet>
    Public Function DevListOrg() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        'Dim _parPotpis As Boolean = False
        Dim _parList As String = ""
        Dim _parGet As String = ""
        Dim _parOrgSfr As String = ""

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "list"
                    _parList = keypair.Value
                Case "get"
                    _parGet = keypair.Value
                Case "sifra"
                    _parOrgSfr = keypair.Value
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _ngApiOrganizacija As New NgApiOrganizacija
        Dim unserializedContent As Object = ""

        Select Case _parList
            Case "byorg"
                _ngApiOrganizacija.GetOrgSifra(_parOrgSfr, 5, 1)

                unserializedContent = String.Join(",", _ngApiOrganizacija.lstOrgSifras)

                If _parGet = "id" Then
                    unserializedContent = String.Join(",", _ngApiOrganizacija.lstOrgIds)
                End If


            Case "bypg"
                _ngApiOrganizacija.GetPGEmpIds(_parOrgSfr)
                unserializedContent = String.Join(",", _ngApiOrganizacija.lstEmpIds)
            Case Else

        End Select






        Return Json(unserializedContent)

    End Function

    ' API L4
    <HttpGet>
    <Authorize>
    Public Function PutNalsDet(ByVal pPutNalID As Integer) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _ApiPutniNalog As New NgApiPnPutniNalog()
        _ApiPutniNalog.getPnPutniNalogDetById(pPutNalID)
        Dim unserializedContent = _ApiPutniNalog.PnPutniNalogDet

        Return Json(unserializedContent)

    End Function

    ' API L5
    <HttpGet>
    <Authorize>
    Public Function PutNalsHead(ByVal pPutNalID As Integer) As IHttpActionResult
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parVrsta As String = ""

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "vrsta"
                    _parVrsta = keypair.Value

            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _ApiPutniNalog As New NgApiPnPutniNalog()
        '_ApiPutniNalog.getPnPutniNalogById(pPutNalID, _parVrsta)
        Dim unserializedContent = _ApiPutniNalog.getPnPutniNalogById(pPutNalID, _ApiSession.EmployeeID, _parVrsta)

        Return Json(unserializedContent)

    End Function

    ' API L6i
    <HttpGet>
    <Authorize>
    Public Function PutNalsIzv(ByVal pPutNalID As Integer) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _ApiPutniNalog As New NgApiPnPutniNalog()
        _ApiPutniNalog.getPnPutniNalogIzvById(pPutNalID)
        Dim unserializedContent = _ApiPutniNalog.PnPutniNalogList.Item(0)

        Return Json(unserializedContent)

    End Function

    ' API L7
    <HttpGet>
    <Authorize>
    Public Function PutNalsStatus() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parPutNaliId As Integer = -1
        Dim _parStatus As String = ""


        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "pPutNalID"
                    _parPutNaliId = Integer.Parse(keypair.Value)
                Case "getstatus"
                    _parStatus = keypair.Value
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        ' Uzmi putninalog
        Dim _ApiPutniNalog As New NgApiPnPutniNalog()
        Dim _putniNalog As ngPnPutniNalog = _ApiPutniNalog.getPnPutniNalogIzvById(_parPutNaliId)


        Select Case _parStatus
            Case "prev"
                _putniNalog.status_naloga = _ApiPutniNalog.getPnPutniNalogStaById("prev", _putniNalog.status_naloga, _putniNalog.id)
            Case "next"
                _putniNalog.status_naloga = _ApiPutniNalog.getPnPutniNalogStaById("next", _putniNalog.status_naloga, _putniNalog.id)
            Case "current"
                _putniNalog.status_naloga = _ApiPutniNalog.getPnPutniNalogStaById(_parStatus, _putniNalog.status_naloga, _putniNalog.id)
        End Select

        Dim unserializedContent = _putniNalog

        Return Json(unserializedContent)

    End Function

    <HttpGet>
    <Authorize>
    Public Function pn_status(ByVal pId As Integer) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _pn_statusQry = New ngPnStatusQuery
        Dim _protokolevidResult = _pn_statusQry.FindOne(pId)

        If _protokolevidResult Is Nothing Then
            Return NotFound()
        End If

        Return Json(_protokolevidResult)

    End Function

    <HttpGet>
    <Authorize>
    Public Function pn_status() As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _pn_status = New ngPnStatusQuery
        Dim _protokolevidLst = _pn_status.GetAll()

        Return Json(_protokolevidLst)

    End Function


    ' API L8i

    <HttpGet>
    <Authorize>
    Public Function PutNalsIzvDet(ByVal pPutNalID As Integer) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _ApiPutniNalog As New NgApiPnPutniNalog()
        _ApiPutniNalog.getPnPutniNalogIzvDetById(pPutNalID)
        '
        ' Uzima detalje izvještaja putnog naloga
        Dim unserializedContent = _ApiPutniNalog.PnPutniNalogDet

        Return Json(unserializedContent)

    End Function

    ' API L9i
    <HttpGet>
    <Authorize>
    Public Function PutNalsIzvDetByOrgRole() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        'Dim _parPotpis As Boolean = False
        Dim _parList As String = ""
        Dim _createdby As Integer = -1
        Dim _parDays As Integer = 0
        Dim _parStatus As String = ""
        Dim _empId As Integer = -1

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "list"
                    _parList = keypair.Value
                Case "createdby"
                    Integer.TryParse(keypair.Value, _createdby)
                Case "days"
                    Integer.TryParse(keypair.Value, _parDays)
                Case "status"
                    _parStatus = keypair.Value
                    '
                    ' Pretvori aktivan, odobren... u 'aktivan','odobren'...
                    '
                    _parStatus = _parStatus.Replace(" ", "")
                    Dim _lstStatus As List(Of String) = _parStatus.Split(",").ToList()
                    Dim _lstStatusFormat As New List(Of String)

                    For Each el As String In _lstStatus
                        el = String.Format("{0}{1}{2}", "'", el, "'")
                        _lstStatusFormat.Add(el)
                    Next

                    _parStatus = String.Join(",", _lstStatusFormat.ToArray())
                Case "empid"
                    Integer.TryParse(keypair.Value, _empId)
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _ngApiOrganizacija As New NgApiOrganizacija
        Dim _ApiPutniNalog As New NgApiPnPutniNalog()

        Select Case _parList
            Case "byorg"


                ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                '
                Dim _roleOrgIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email)
                '
                ' Uzmi rekurzivno sve org id u hijerarhiji organizacije
                _ngApiOrganizacija.GetOrgSifra(_roleOrgIds, 5, 1)

                Dim _orgIds As String = String.Join(",", _ngApiOrganizacija.lstOrgSifras)
                Dim _tmpTable As String = "tmp_" + _ApiSession.EmployeeID.ToString + "_putnalozi"

                'Dim _ApiPutniNalog As New NgPnApiPutniNalog()
                If _orgIds.Length > 0 Then
                    _ApiPutniNalog.getPnOrgPutniNalogIzvDet(_orgIds, _tmpTable, _parStatus, _ApiSession.EmployeeID, _parDays, _createdby)
                End If


            Case "bypg"

                ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                '
                Dim _rolePGIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email, True)
                '
                _ngApiOrganizacija.GetPGEmpIds(_rolePGIds)
                Dim _PGIds As String = String.Join(",", _ngApiOrganizacija.lstEmpIds)
                Dim _tmpTable As String = "tmp_" + _ApiSession.EmployeeID.ToString + "_putnalozi"

                'Dim _ApiPutniNalog As New NgPnApiPutniNalog()
                If _PGIds.Length > 0 Then
                    _ApiPutniNalog.getPnPGPutniNalogIzvDet(_PGIds, _tmpTable, _parStatus, _ApiSession.EmployeeID, _parDays, _createdby)
                End If

            Case "byorgpg"


                ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                '
                Dim _roleOrgIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email)
                '
                ' Uzmi rekurzivno sve org id u hijerarhiji organizacije
                _ngApiOrganizacija.GetOrgSifra(_roleOrgIds, 5, 1)
                Dim _orgIds As String = String.Join(",", _ngApiOrganizacija.lstOrgSifras)

                Dim _rolePGIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email, True)
                _ngApiOrganizacija.GetPGEmpIds(_rolePGIds)
                Dim _PGIds As String = String.Join(",", _ngApiOrganizacija.lstEmpIds)

                Dim _tmpTable As String = "tmp_" + _ApiSession.EmployeeID.ToString + "_putnalozi"

                'Dim _ApiPutniNalog As New NgPnApiPutniNalog()
                If _orgIds.Length > 0 Or _PGIds.Length > 0 Then
                    _ApiPutniNalog.getPnOrgPGPutniNalogIzvDet(_orgIds, _PGIds, _tmpTable, _parStatus, _ApiSession.EmployeeID, _parDays, _createdby)
                End If

            Case "byupo"


                ' Lista putnih naloga po uposleniku: _empId
                '
                Dim _roleOrgIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email)
                '
                Dim _orgIds As String = ""
                Dim _PGIds As String = _empId.ToString()

                Dim _tmpTable As String = "tmp_" + _ApiSession.EmployeeID.ToString + "_putnalozi"

                'Dim _ApiPutniNalog As New NgPnApiPutniNalog()
                If _orgIds.Length > 0 Or _PGIds.Length > 0 Then
                    _ApiPutniNalog.getPnOrgPGPutniNalogIzvDet(_orgIds, _PGIds, _tmpTable, _parStatus, _ApiSession.EmployeeID, _parDays, _createdby)
                End If
        End Select






        Dim unserializedContent = _ApiPutniNalog.PnPutniNalogIzvDetList

        Return Json(unserializedContent)

    End Function


    ' API C1
    <HttpPost>
    <Authorize>
    Public Function PutNalsIns(<FromBody()> ByVal valuePnPutNal As ngPnPutniNalog) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _ngPnPutNal As ngPnPutniNalog = valuePnPutNal
        Dim _newPutNalID As Integer = -1

        Dim _ApiPutniNalog As New NgApiPnPutniNalog()

        'Dim _RolesAllowed() As String = {"uposlenik", "sekretarica", "rukovodilac", "producent", "likvidatura", "uprava"}

        Dim _ExtRoles() As String = {"sekretarica-ext", "rukovodilac-ext", "producent-ext", "likvidatura-ext", "uprava-ext"}

        '
        ' Za ext role status 0 mijenja u 10, a status 1 u 11
        '
        If _ExtRoles.Contains(_ApiSession.Role) Then
            Select Case _ngPnPutNal.vrsta_naloga
                Case 0
                    _ngPnPutNal.vrsta_naloga = _ngPnPutNal.vrsta_naloga + 10
                Case 1
                    _ngPnPutNal.vrsta_naloga = _ngPnPutNal.vrsta_naloga + 10
                Case 10, 11

                Case Else
                    _ngPnPutNal.vrsta_naloga = 10
            End Select

        End If

        'If _RolesAllowed.Contains(_ApiSession.Role) Then
        _newPutNalID = _ApiPutniNalog.insPutNalNalog(_ngPnPutNal)

        If _newPutNalID > 0 Then
            _ngPnPutNal.id = _newPutNalID

            Select Case _ngPnPutNal.status_naloga
                Case "akontacija"
                    _ngPnPutNal.status_naloga = "akontacija"
                Case "akontacijaPF"
                    _ngPnPutNal.status_naloga = "akontacijaPF"
                Case "pripremaPF"
                    _ngPnPutNal.status_naloga = "pripremaPF"
                Case Else
                    _ngPnPutNal.status_naloga = "priprema"
            End Select


            _ApiPutniNalog.insPutNalNalogIzv(_ngPnPutNal.id)
            _ApiPutniNalog.insPutNalNalogDetIzv(_ngPnPutNal.id)
            _ApiPutniNalog.insPutNalNalogDet(_ngPnPutNal.id)

            'TODO: Novi nalog se insertuje sa statusom 0, da li ovo treba??
            _ApiPutniNalog.updPutNalNalog(_ngPnPutNal.id, _ngPnPutNal)

            _ApiPutniNalog.insPutNalNalogUser(_ngPnPutNal, _ApiSession.Email)
            _ApiPutniNalog.updPutNalStatus(_ngPnPutNal.id, _ngPnPutNal.status_naloga, _ApiSession.EmployeeID)

        End If
        'End If


        Dim unserializedContent = _ApiPutniNalog.getPnPutniNalogById(_ngPnPutNal.id, _ApiSession.EmployeeID)

        Return Json(unserializedContent)

    End Function


    ' API S1, S1A, S2, S4i
    <HttpPost>
    <Authorize>
    Public Function PutNalWrite(ByVal action As String, <FromBody()> ByVal valuePnPutNal As ngPnPutniNalog) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        '
        ' Uzmi iz <body> parametar
        '
        Dim _replyMess As ngPnPutniNalog = valuePnPutNal
        Dim _ngPnPutNal As ngPnPutniNalog = valuePnPutNal


        Dim _ExtRoles() As String = {}

        Select Case action
            Case "status"
                '
                ' DONE: Provjera da li ima pravo promjene statusa putnog naloga
                '
                Dim _RolesAllowed() As String = {}
                ' TODO: uvesti dozvoljene role u MySql tabelu - putninalog_workflow

                Select Case _ngPnPutNal.status_naloga
                    Case "priprema"
                        _RolesAllowed = {"sekretarica", "rukovodilac", "producent", "direkcija", "uprava", "likvidatura"}
                        _ExtRoles = {"ext-sekretarica", "ext-rukovodilac", "ext-producent", "ext-likvidatura", "ext-uprava"}
                    Case "pripremaPF"
                        _RolesAllowed = {"sekretarica", "rukovodilac", "producent", "direkcija", "uprava", "likvidatura"}
                    Case "akontacija"
                        _RolesAllowed = {"sekretarica", "rukovodilac", "producent", "direkcija", "uprava", "likvidatura"}
                        _ExtRoles = {"ext-sekretarica", "ext-rukovodilac", "ext-producent", "ext-likvidatura", "ext-uprava"}
                    Case "akontacijaPF"
                        _RolesAllowed = {"sekretarica", "rukovodilac", "producent", "direkcija", "uprava", "likvidatura"}
                    Case "prevoz"
                        _RolesAllowed = {"direkcija", "uprava", "likvidatura"}
                        _ExtRoles = {"ext-sekretarica", "ext-rukovodilac", "ext-producent", "ext-likvidatura", "ext-uprava"}
                    Case "otvoren"
                        _RolesAllowed = {"sekretarica", "rukovodilac", "producent", "direkcija", "uprava", "likvidatura"}
                        _ExtRoles = {"ext-sekretarica", "ext-rukovodilac", "ext-producent", "ext-likvidatura", "ext-uprava"}
                    Case "otvorenPF"
                        _RolesAllowed = {"sekretarica", "rukovodilac", "producent", "direkcija", "uprava", "likvidatura"}
                    Case "odobren"
                        _RolesAllowed = {"rukovodilac", "direkcija", "uprava", "blagajna", "likvidatura", "sekretarica", "producent"}
                        _ExtRoles = {"ext-sekretarica", "ext-rukovodilac", "ext-producent", "ext-likvidatura", "ext-uprava"}
                    Case "storno"
                        _RolesAllowed = {"rukovodilac", "direkcija", "uprava", "blagajna", "likvidatura", "sekretarica", "producent"}
                    Case "odobrenPF"
                        _RolesAllowed = {"rukovodilac", "direkcija", "uprava", "blagajna", "likvidatura", "sekretarica", "producent"}
                    Case "stornoPF"
                        _RolesAllowed = {"rukovodilac", "direkcija", "uprava", "blagajna", "likvidatura", "sekretarica", "producent"}
                    Case "aktivan", "opravdan"
                        _RolesAllowed = {"blagajna", "direkcija", "uprava", "rukovodilac", "uposlenik", "sekretarica"}
                        'TODO Uposlenik i rukovodialc mogu setovati aktivan - samo ako je trenutni status naloga popunjen
                    Case "popunautoku"
                        _RolesAllowed = {"uposlenik", "sekretarica", "rukovodilac", "producent", "direkcija", "uprava"}
                    Case "popunjen"
                        _RolesAllowed = {"uposlenik", "sekretarica", "rukovodilac", "producent", "direkcija", "uprava", "likvidatura"}
                    Case "provjeren"
                        _RolesAllowed = {"rukovodilac", "direkcija", "uprava", "likvidatura", "upravafin"}
                    Case "obracunutoku"
                        _RolesAllowed = {"likvidatura", "blagajna", "direkcija", "uprava", "upravafin"}
                    Case "obracunfin"
                        _RolesAllowed = {"rukovodilac", "direkcija", "uprava", "likvidatura", "upravafin", "blagajna"}
                    Case "obracunat"
                        _RolesAllowed = {"likvidatura", "blagajna", "direkcija", "uprava", "upravafin", "blagajna"}
                    Case "opravdan"
                        _RolesAllowed = {"blagajna", "direkcija", "uprava"}
                    Case "arhiviran"
                        _RolesAllowed = {"blagajna", "likvidatura", "direkcija", "uprava"}
                    Case "storno"
                        _RolesAllowed = {"sekretarica", "rukovodilac", "producent"}
                End Select

                Dim _ApiPutniNalog As New NgApiPnPutniNalog()

                If _RolesAllowed.Contains(_ApiSession.Role) Or _ExtRoles.Contains(_ApiSession.Role) Then

                    Dim _currentStatus = _ApiPutniNalog.getPnPutniNalogStaById("current", _ngPnPutNal.status_naloga, _ngPnPutNal.id)

                    ' Redovni nalozi
                    '
                    If (_currentStatus <> _ngPnPutNal.status_naloga) And _RolesAllowed.Contains(_ApiSession.Role) Then
                        _ApiPutniNalog.updPutNalStatus(_ngPnPutNal.id, _ngPnPutNal.status_naloga, _ApiSession.EmployeeID)

                        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
                        '   SLANJE EMAIL PORUKE O PROMJENI STATUSA

                        Dim _apiEmailNotify = New ngApiEmailNotify(_ApiSession)
                        Dim _emailNotifyOk = _apiEmailNotify.prepForEmailByEmp(_ngPnPutNal.id, _ngPnPutNal.status_naloga, _ngPnPutNal.vrsta_naloga, _ApiSession.EmployeeID)

                        If _emailNotifyOk = True Then
                            Dim _ApiEmails As New NgApiEmailNET(_ApiSession, "info@mperun.net", "Perun - poslovni portal BHRT")
                            _ApiEmails.SendMail(_apiEmailNotify, _ngPnPutNal.id, _ApiSession.EmployeeID)
                        End If

                        'Dim _ApiEmails As New NgApiEmailNET(_ApiSession, "info@mperun.net", "Perun - poslovni portal BHRT")
                        '_ApiEmails.SendMail(_apiEmailNotify, _ngPnPutNal.id)
                    End If
                    '
                    ' Externi nalozi
                    '
                    If (_currentStatus <> _ngPnPutNal.status_naloga) And (_ExtRoles.Contains(_ApiSession.Role)) _
                        And (_ngPnPutNal.vrsta_naloga = 10 Or _ngPnPutNal.vrsta_naloga = 11) Then
                        _ApiPutniNalog.updPutNalStatus(_ngPnPutNal.id, _ngPnPutNal.status_naloga, _ApiSession.EmployeeID)


                        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
                        '   SLANJE EMAIL PORUKE O PROMJENI STATUSA

                        Dim _apiEmailNotify = New ngApiEmailNotify(_ApiSession)
                        Dim _emailNotifyOk = _apiEmailNotify.prepForEmailByEmp(_ngPnPutNal.id, _ngPnPutNal.status_naloga, _ngPnPutNal.vrsta_naloga, _ApiSession.EmployeeID)

                        If _emailNotifyOk = True Then
                            Dim _ApiEmails As New NgApiEmailNET(_ApiSession, "info@mperun.net", "Perun - poslovni portal BHRT")
                            _ApiEmails.SendMail(_apiEmailNotify, _ngPnPutNal.id, _ApiSession.EmployeeID)
                        End If

                    End If
                End If
                _replyMess = _ApiPutniNalog.getPnPutniNalogById(_ngPnPutNal.id, _ApiSession.EmployeeID)

            Case "nalog"
                ' TODO: Ako dobije naloga bez statusa (novi nalog), ne upiše podatak u tabelu putninalog_user
                Dim _RolesAllowed() As String = {}

                Select Case _ngPnPutNal.status_naloga
                    Case "priprema", "otvoren", "akontacija", "pripremaPF", "otvorenPF", "akontacijaPF"
                        _RolesAllowed = {"uposlenik", "sekretarica", "rukovodilac", "producent", "uprava", "likvidatura", "blagajna"}
                        _ExtRoles = {"ext-sekretarica", "ext-rukovodilac", "ext-producent", "ext-likvidatura", "ext-uprava"}
                    Case "odobren"
                        _RolesAllowed = {"blagajna"}
                        ' TODO: Provjera da li ima e-potpis, ako ima zabrana pisanja
                End Select

                Dim _ApiPutniNalog As New NgApiPnPutniNalog()

                If _RolesAllowed.Contains(_ApiSession.Role) Then
                    _ApiPutniNalog.updPutNalNalog(_ngPnPutNal.id, _ngPnPutNal)
                End If

                If (_ExtRoles.Contains(_ApiSession.Role)) And (_ngPnPutNal.vrsta_naloga = 10 Or _ngPnPutNal.vrsta_naloga = 11) Then
                    _ApiPutniNalog.updPutNalNalog(_ngPnPutNal.id, _ngPnPutNal)
                End If


                _replyMess = _ApiPutniNalog.getPnPutniNalogById(_ngPnPutNal.id, _ApiSession.EmployeeID)

                '
                ' API S4i
            Case "ispl_akontacije"
                ' Blagajna ažurira ispl_akontacije

                Dim _ApiPutniNalog As New NgApiPnPutniNalog()

                _ApiPutniNalog.updPutNalAkontacija(_ngPnPutNal.id, _ngPnPutNal.ispl_akontacije)

                _replyMess = _ApiPutniNalog.getPnPutniNalogById(_ngPnPutNal.id, _ApiSession.EmployeeID)

                '
                ' API S4i
            Case "izvjestaj"
                Dim _ApiPutniNalog As New NgApiPnPutniNalog()
                _ApiPutniNalog.updPutNalIzv(_ngPnPutNal.id, _ngPnPutNal)
                _replyMess = _ApiPutniNalog.getPnPutniNalogIzvById(_ngPnPutNal.id)

            Case Else

        End Select

        Return Json(_replyMess)

    End Function

    ' API S3
    <HttpPost>
    <Authorize>
    Public Function PutNalDetWrite(ByVal pPutNalID As Integer, <FromBody()> ByVal valuePnPutNalDet As List(Of NgPnPutniNalogDet)) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _ngPnPutNal As List(Of NgPnPutniNalogDet) = valuePnPutNalDet


        Dim _ApiPutniNalog As New NgApiPnPutniNalog()

        '
        ' Ako je id=-1, INSERT novog reda
        '
        For Each el As NgPnPutniNalogDet In valuePnPutNalDet
            If el.id > 0 Then
                ' Change
                _ApiPutniNalog.updPutNalNalogDet(el.id, el)
            ElseIf el.id = -1 Then
                _ApiPutniNalog.insPutNalNalogDet(el.id, el)
            ElseIf el.id = -1 Then
                ' Insert
                _ApiPutniNalog.insPutNalNalogDet(el.id, el)
            ElseIf el.id < 0 And el.pn_id < 0 Then
                ' Delete
                ' el.id je (-)
                _ApiPutniNalog.delPutNalNalogDet(pPutNalID, -el.id)
            End If
        Next
        '
        ' Ažuriraj sume u putninaloh
        '
        _ApiPutniNalog.updPutNalDetSuma(pPutNalID)

        '_ApiPutniNalog.updPutNalStatus(_ngPnPutNal.id, _ngPnPutNal.status_naloga)
        _ApiPutniNalog.getPnPutniNalogDetById(pPutNalID)
        Dim _replyMess = _ApiPutniNalog.PnPutniNalogDet



        Return Json(_replyMess)

    End Function

    ' API S5i
    <HttpPost>
    <Authorize>
    Public Function PutNalIzvDetWrite(ByVal pPutNalID As Integer, <FromBody()> ByVal valuePnPutNalDet As List(Of NgPnPutniNalogDet)) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _ngPnPutNal As List(Of NgPnPutniNalogDet) = valuePnPutNalDet


        Dim _ApiPutniNalog As New NgApiPnPutniNalog()

        '
        ' Ako je id=-1, INSERT novog reda
        '
        ' Prvo provjera da li već ima insertovan izvještaj
        ' id putnog naloga uzima iz el
        '
        Dim _ApiPutniNalog2 As New NgApiPnPutniNalog()
        _ApiPutniNalog2.getPnPutniNalogIzvDetById(pPutNalID)

        For Each el As NgPnPutniNalogDet In valuePnPutNalDet
            If el.id = -1 Then
                ' Insert
                '                '
                ' Ako nema radova u _ApiPutniNalog2.PnPutniNalogDet, onda INSERT
                '
                If _ApiPutniNalog2.PnPutniNalogDet.Count = 0 Then
                    _ApiPutniNalog.insPutNalNalogIzvDet(el.id, el)
                End If
            End If
        Next

        '
        ' Ako je id<0 DELETE, inače UPDATE
        '
        For Each el As NgPnPutniNalogDet In valuePnPutNalDet
            If el.id > 0 Then
                ' Change
                _ApiPutniNalog.updPutNalNalogIzvDet(el.id, el)
            ElseIf el.id < 0 And el.pn_id < 0 Then
                ' Delete
                ' el.id je (-)
                _ApiPutniNalog.delPutNalNalogIzvDet(pPutNalID, -el.id)
            End If
        Next
        '_ApiPutniNalog.updPutNalStatus(_ngPnPutNal.id, _ngPnPutNal.status_naloga)
        _ApiPutniNalog.getPnPutniNalogIzvDetById(pPutNalID)
        Dim _replyMess = _ApiPutniNalog.PnPutniNalogDet



        Return Json(_replyMess)

    End Function




    <HttpPost>
    <Authorize>
    Public Function PutNalWriteGroup(ByVal action As String, ByVal ids As String, <FromBody()> ByVal valuePnPutNal As ngPnPutniNalog) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        '
        ' Uzmi iz post parametara...
        '
        Dim _replyMess As ngPnPutniNalog = valuePnPutNal

        Dim _idsArr As String() = ids.Split(",")

        Select Case action
            Case "akontacija"

                Dim _ApiPutniNalog As New NgApiPnPutniNalog()
                Dim _ngPnPutNal As ngPnPutniNalog

                Dim _RolesAllowed() As String = {}
                _RolesAllowed = {"likvidatura"}
                If Not _RolesAllowed.Contains(_ApiSession.Role) Then
                    Return New Results.StatusCodeResult(HttpStatusCode.BadRequest, Request)
                End If

                For Each el In _idsArr
                    _ngPnPutNal = _ApiPutniNalog.getPnPutniNalogById(Convert.ToInt32(el), _ApiSession.EmployeeID)
                    _ngPnPutNal.iznos_dnevnice = valuePnPutNal.iznos_dnevnice
                    _ngPnPutNal.proc_dnevnice = valuePnPutNal.proc_dnevnice
                    _ngPnPutNal.broj_dnevnica = valuePnPutNal.broj_dnevnica
                    _ngPnPutNal.akontacija_dnevnice = valuePnPutNal.akontacija_dnevnice
                    _ngPnPutNal.akontacija_nocenje = valuePnPutNal.akontacija_nocenje
                    _ngPnPutNal.akontacija_ostalo = valuePnPutNal.akontacija_ostalo
                    _ngPnPutNal.iznos_akontacije = valuePnPutNal.iznos_akontacije
                    _ngPnPutNal.ispl_akontacije = valuePnPutNal.ispl_akontacije

                    Select Case _ngPnPutNal.status_naloga
                        Case "akontacija"
                            Dim _updStatus As Boolean = False
                            _updStatus = _ApiPutniNalog.updPutNalNalog(_ngPnPutNal.id, _ngPnPutNal)

                            If _updStatus = False Then Return New Results.StatusCodeResult(HttpStatusCode.BadRequest, Request)

                        Case Else

                    End Select
                Next



                '_replyMess = _ApiPutniNalog.getPnPutniNalogById(_ngPnPutNal.id)


            Case Else

        End Select

        Return Json(_replyMess)

    End Function

    <HttpPost>
    <Authorize>
    Public Function PutNalWriteBlagajna(ByVal pPutNalID As String, <FromBody()> ByVal valuePnPutNal As ngPnPutniNalog) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        '
        ' Uzmi iz post parametara...
        '
        Dim _replyMess As ngPnPutniNalog = valuePnPutNal


        Dim _ApiPutniNalog As New NgApiPnPutniNalog()
        Dim _ngPnPutNal As ngPnPutniNalog

        _ngPnPutNal = _ApiPutniNalog.getPnPutniNalogById(Convert.ToInt32(pPutNalID), _ApiSession.EmployeeID)

        _ngPnPutNal.UkIznosObracuna = valuePnPutNal.UkIznosObracuna
        _ngPnPutNal.UkIznosRazlika = valuePnPutNal.UkIznosRazlika

        Dim _updStatus As Boolean = False
        _updStatus = _ApiPutniNalog.updPutNalNalog(_ngPnPutNal.id, _ngPnPutNal)

        If _updStatus = False Then Return New Results.StatusCodeResult(HttpStatusCode.BadRequest, Request)



        Return Json(_ngPnPutNal)

    End Function

    ' API L8i
    <HttpGet>
    <Authorize>
    Public Function PutNalPrilog(ByVal pPutNalID As Integer) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(Me.GetType().Name, MethodBase.GetCurrentMethod().Name, _ApiSession.Email, _ApiSession.Role, _comment)

        Dim _ApiPnPrilog As New ngApiPnPrilog()


        Dim _replyMess = _ApiPnPrilog.selPrilog(pPutNalID)
        Return Json(_replyMess)

    End Function


    ' API S3
    <HttpPost>
    <Authorize>
    Public Function PutNalPrilogWrite(ByVal pPutNalID As Integer, <FromBody()> ByVal valuePnPrilog As List(Of ngPnPrilog)) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _ApiPnPrilog As New ngApiPnPrilogObracun()

        '
        ' Change, Insert, Delete based on id value...
        ' If you set a column to the value it currently has, MySQL notices this and does not update it. 
        ' https://dev.mysql.com/doc/refman/5.7/en/update.html
        '
        For Each el As ngPnPrilog In valuePnPrilog
            If el.id > 0 Then
                ' Change
                _ApiPnPrilog.updPrilog(el.id, el)
            ElseIf el.id = -1 Then
                ' Insert
                _ApiPnPrilog.insPrilog(el.id, el)
            ElseIf el.id < 0 And el.pn_id < 0 Then
                ' Delete - el.id je (-)
                el.id = -el.id                        ' -id dolazi od API poziva 
                el.pn_id = -el.pn_id                  ' -pn_id dolazi od API poziva 
                _ApiPnPrilog.delPrilog(el.id, el)
            End If
        Next

        If valuePnPrilog.Count > 0 Then
            _ApiPnPrilog.updIzvObracun(pPutNalID)
        End If

        Dim _replyMess = _ApiPnPrilog.selPrilog(pPutNalID)
        Return Json(_replyMess)

    End Function


    ' API L8i
    <HttpGet>
    <Authorize>
    Public Function PutNalPrilogObrWrite(ByVal pPutNalID As Integer, ByVal pID As Integer, ByVal pPutNalObrID As Integer) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(Me.GetType().Name, MethodBase.GetCurrentMethod().Name, _ApiSession.Email, _ApiSession.Role, _comment)

        Dim _ApiPnPrilog As New ngApiPnPrilogObracun()



        If pPutNalObrID > 0 Then
            ' Change
            _ApiPnPrilog.updPrilogObracun(pID, pPutNalID)
        ElseIf pPutNalObrID = -1 Then
            ' Insert
            _ApiPnPrilog.insPrilogObracun(pID, pPutNalID)
        ElseIf pPutNalObrID < 0 And pPutNalID < 0 Then
            ' Delete
            _ApiPnPrilog.delPrilogObracun(pID, -pPutNalID)
            pPutNalID = -pPutNalID
        End If



        Dim _replyMess = _ApiPnPrilog.selPrilog(pPutNalID)
        Return Json(_replyMess)

    End Function

    ' API S6
    <HttpPost>
    <Authorize>
    Public Function PutNalColWrite(<FromBody()> ByVal _ngPnPutNal As List(Of ngPnPutniNalog)) As IHttpActionResult
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parColName As String = ""

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "colname"
                    _parColName = keypair.Value
            End Select
        Next


        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _ApiPutniNalog As New NgApiPnPutniNalog()
        Dim _replyMess As Object = False

        Select Case _parColName
            Case "org_jed"
                For Each el As ngPnPutniNalog In _ngPnPutNal
                    If el.id > 0 Then
                        ' Change
                        _replyMess = _ApiPutniNalog.updPutNalNalogCol(el.id, el, _parColName)
                    Else

                    End If
                Next

                Return Json(_replyMess)

            Case "temeljnica"
                For Each el As ngPnPutniNalog In _ngPnPutNal
                    If el.id > 0 Then
                        ' Change
                        _replyMess = _ApiPutniNalog.updPutNalNalogTem(el.id, el)
                    Else

                    End If
                Next


                '_ApiPutniNalog.updPutNalStatus(_ngPnPutNal.id, _ngPnPutNal.status_naloga)
                '_ApiPutniNalog.getPnPutniNalogDetById(pPutNalID)
                '_replyMess = _ApiPutniNalog.PnPutniNalogDet

                Return Json(_replyMess)

            Case Else
                Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
        End Select


    End Function

    ' API D1
    <HttpPost>
    <Authorize>
    Public Function PutNalsDel(ByVal pPutNalID As Integer) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _ApiPutniNalog As New NgApiPnPutniNalog()
        Dim unserializedContent = _ApiPutniNalog.delPutNalNalog(pPutNalID)

        Return Json(unserializedContent)

    End Function


    ' API D2
    <HttpPost>
    <Authorize>
    Public Function PutNalsDetDel(ByVal pPutNalID As Integer, ByVal pID As Integer) As IHttpActionResult
        '
        ' pID - id table row putninalog_obracun
        '
        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _ApiPutniNalog As New NgApiPnPutniNalog()
        Dim unserializedContent = _ApiPutniNalog.delPutNalNalogDet(pPutNalID, pID)

        '
        ' Ažuriraj sume u putninaloh
        '
        _ApiPutniNalog.updPutNalDetSuma(pPutNalID)

        Return Json(unserializedContent)

    End Function

    ' API D3i
    <HttpPost>
    <Authorize>
    Public Function PutNalsIzvDetDel(ByVal pPutNalID As Integer, ByVal pID As Integer) As IHttpActionResult
        '
        ' pID - id table row putninalog_obracun
        '
        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _ApiPutniNalog As New NgApiPnPutniNalog()
        Dim unserializedContent = _ApiPutniNalog.delPutNalNalogIzvDet(pPutNalID, pID)


        Return Json(unserializedContent)

    End Function



    ' API F1
    ' Test - unauthorized
    '
    <HttpGet>
    Public Function GetFile() As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _path = Directory.GetCurrentDirectory()
        Dim _reqFile = HttpContext.Current.Server.MapPath("~/Data/pdf-test.pdf")
        Dim _dataBytes As Byte()
        Dim _fileName As String = "test.pdf"

        Dim _apiReport As New ngApiReport

        _apiReport.setupReport()
        '_reqFile = String.Format("C:\Users\aa\Documents\Visual Studio 2015\WebSites\SyncfusionPlaNETApi\Data\report-test.pdf")
        _reqFile = HttpContext.Current.Server.MapPath("~/Data/report-test.pdf")

        _dataBytes = File.ReadAllBytes(_reqFile)
        Dim dataStream = New MemoryStream(_dataBytes)

        Return New ngAPIFileResult(dataStream, Request, _fileName)

    End Function

    ' API F2
    ' CRYSTAL REPORT
    '
    <HttpGet>
    <Authorize>
    Public Function GetFile2(ByVal pPutNalId As Integer) As IHttpActionResult
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()


        Dim _parReportCode As String = "PTNL1"
        Dim _parDataCode As String = "PTNL1"


        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "parReportCode"
                    _parReportCode = keypair.Value
                Case "parDataCode"
                    _parDataCode = keypair.Value
            End Select
        Next


        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '

        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _apiReport As New ngApiReport

        Dim _reportFile = _apiReport.createReport(pPutNalId, _parReportCode, _parDataCode)
        Dim _reqFile As String = HttpContext.Current.Server.MapPath("~/Data/" + _reportFile)


        Dim _dataBytes As Byte() = File.ReadAllBytes(_reqFile)
        Dim dataStream = New MemoryStream(_dataBytes)

        Return New ngAPIFileResult(dataStream, Request, _reportFile)

    End Function


    ' API F3
    ' CRYSTAL REPORT - vraća ime reporta za putni nalog sa Id 
    '
    <HttpGet>
    Public Function GetReportName(ByVal pPutNalId As Integer) As IHttpActionResult
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' READ GET parameters
        '
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parReportCode As String = "PTNL1"
        Dim _parDataCode As String = "PTNL1"
        Dim _parPdfRemove As String = ""

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "parReportCode"
                    _parReportCode = keypair.Value
                Case "parDataCode"
                    _parDataCode = keypair.Value
                Case "pdfRemove"
                    _parPdfRemove = keypair.Value
            End Select
        Next


        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' LOG SESIJA
        '
        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)
        _comment = String.Format("Api poziv GetReportName...pnId: {0}...{1}-{2}", pPutNalId.ToString, _parReportCode, _parDataCode)
        _ApiSession.insApiLog(_log, _comment)
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' KREIRANJE REPORT FAJLA
        '
        Dim _ApiPutniNalog As New NgApiPnPutniNalog()
        Dim _apiReport As New ngApiReport

        Dim _reqFile As String = _apiReport.getRandomFileName(pPutNalId)
        Dim _destDir As String = ngApiPath.getCurrentPath()
        'Dim _destDir As String = ngApiPath.getTempPath()



        Dim _pnDoc As Integer = 0
        If _parReportCode = "PTNL1" AndAlso _parDataCode = "PTNL1" Then
            _pnDoc = 1
        ElseIf _parReportCode = "PTNL2" AndAlso _parDataCode = "PTNL2" Then
            _pnDoc = 2
        End If

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' Uzmi fajl iz foldera PutNal => ako nema, vrati ""
        '
        Dim _ApiPNalog As New NgApiPnPutniNalog()
        Dim _lastSignedFile As String = _ApiPNalog.getLastSignedFile(pPutNalId, _pnDoc)
        '
        '
        If _lastSignedFile <> "" Then

            My.Computer.FileSystem.CopyFile(_lastSignedFile, _destDir + _reqFile, overwrite:=False)
        Else

            _reqFile = _apiReport.createReport(pPutNalId, _parReportCode, _parDataCode)

            _comment = String.Format("Api poziv GetReportName...pnId: {0}", _reqFile)
            _ApiSession.insApiLog(_log, _comment)
        End If

        If _parPdfRemove <> "" Then

            'Load the PDF document.
            Dim loadedDocument As New PdfLoadedDocument(_destDir + _reqFile)

            'Remove the first page in the PDF document
            loadedDocument.Pages.RemoveAt(0)

            'Save the document.
            loadedDocument.Save("Output.pdf")

            'Close the document.
            loadedDocument.Close(True)

        End If

        Return Json(_reqFile)

    End Function

    '
    ' DNEVNIK RADA
    '
    <HttpGet>
    Public Function GetReportName() As IHttpActionResult
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _ReportCode As String = ""
        Dim _DataCode As String = ""
        Dim _procParam As New List(Of String)

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "parReportCode"
                    _ReportCode = keypair.Value
                Case "parDataCode"
                    _DataCode = keypair.Value
                Case Else
                    _procParam.Add(keypair.Value)
                    ' TODO: ovo tretira samo numeric parametre - razviti opciju i za string parametre
            End Select
        Next


        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)



        Dim _apiReport As New ngApiReport
        Dim _reqFile = _apiReport.createReport(_procParam, _ReportCode, _DataCode)

        Return Json(_reqFile)

    End Function

    ' API F4 - vraća sadržaj reporta kao BLOB
    <HttpGet>
    <Authorize>
    Public Function GetReport() As IHttpActionResult
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        'Dim _parEmpID As Integer = -1

        Dim _parPutNalId As Integer = -1
        Dim _parFileName As String = Nothing

        Dim _parReportCode As String = "PTNL1"
        Dim _parDataCode As String = "PTNL1"

        Dim _parPageRemove As Integer = -1

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key.ToLower()
                'Case "pEmployeeID".ToLower()
                '    If Not (Integer.TryParse(keypair.Value, _parEmpID) = True) Then
                '        _parEmpID = -2
                '    End If
                Case "pPutNalId".ToLower()
                    If Not (Integer.TryParse(keypair.Value, _parPutNalId) = True) Then
                        _parPutNalId = -2
                    End If
                Case "pFileName".ToLower()
                    _parFileName = keypair.Value
                Case "parReportCode".ToLower()
                    _parReportCode = keypair.Value
                Case "parDataCode".ToLower()
                    _parDataCode = keypair.Value
                Case "pPageRemove".ToLower()
                    If Not (Integer.TryParse(keypair.Value, _parPageRemove) = True) Then
                        _parPageRemove = -1
                    End If
            End Select
        Next

        '-------------------------------------------------------------------------------------------------------
        ' SESSION LOG 
        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)

        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)
        '-------------------------------------------------------------------------------------------------------

        Dim dataStream As MemoryStream
        Dim _reportFile As String


        If _parFileName IsNot Nothing Then              ' Vrati report koji je prethodno generisan i spašen na server

            _reportFile = _parFileName

        Else                                            ' Kreiraj report 
            ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            ' KREIRANJE REPORT FAJLA
            '
            Dim _ApiPutniNalog As New NgApiPnPutniNalog()
            'Dim _eSignPos As Integer = 0
            Dim _apiReport As New ngApiReport

            Dim _reqFile As String = _apiReport.getRandomFileName(_parPutNalId)
            Dim _destDir As String = ngApiPath.getCurrentPath()

            '_eSignPos = _ApiPutniNalog.getLastSignedByPos(_parPutNalId, 1, 1)

            Dim _pnDoc As Integer = 0
            If _parReportCode = "PTNL1" AndAlso _parDataCode = "PTNL1" Then
                _pnDoc = 1
            ElseIf _parReportCode = "PTNL2" AndAlso _parDataCode = "PTNL2" Then
                _pnDoc = 2
            End If

            Dim _ApiPNalog As New NgApiPnPutniNalog()
            Dim _lastSignedFile As String = _ApiPNalog.getLastSignedFile(_parPutNalId, _pnDoc)
            '
            Dim _apiDbFiles As New ngApiDbFiles
            '
            If _lastSignedFile <> "" Then

                My.Computer.FileSystem.CopyFile(_lastSignedFile, _destDir + _reqFile, overwrite:=False)
            Else
                _reqFile = _apiReport.createReport(_parPutNalId, _parReportCode, _parDataCode)
            End If

            _reportFile = _reqFile
        End If

        Dim documentPath As String = GetDocumentPath(_reportFile)


        If _parPageRemove > -1 Then

            Dim loadedDocument As New PdfLoadedDocument(documentPath)
            loadedDocument.Pages.RemoveAt(_parPageRemove)


            dataStream = New MemoryStream()

            loadedDocument.Save(dataStream)
            loadedDocument.Close(True)



            Dim bytes As Byte() = dataStream.ToArray()
            dataStream = New MemoryStream(bytes)

            Return New ngAPIFileResult(dataStream, Request, _reportFile)
        End If


        If Not String.IsNullOrEmpty(documentPath) Then

            Dim bytes As Byte() = System.IO.File.ReadAllBytes(documentPath)
            dataStream = New MemoryStream(bytes)
        Else
            Return Json(_reportFile + " is not found")
        End If




        Return New ngAPIFileResult(dataStream, Request, _reportFile)

    End Function

    ' API F4 - vraća sadržaj reporta kao BLOB
    <HttpGet>
    <Authorize>
    Public Function GetReportMod() As IHttpActionResult

        '-------------------------------------------------------------------------------------------------------
        ' Uzima parametre GET
        '
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parPutNalId As Integer = -1
        Dim _parPutNalDoc As Integer = -1
        Dim _parMode As String = Nothing

        Dim _checkPars As Boolean = True

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key.ToLower()
                Case "pPutNalId".ToLower()
                    _checkPars = _checkPars And Integer.TryParse(keypair.Value, _parPutNalId)

                Case "pPutNalDoc".ToLower()
                    _checkPars = _checkPars And Integer.TryParse(keypair.Value, _parPutNalDoc)

                Case "pMode".ToLower()
                    _parMode = keypair.Value
            End Select
        Next

        If _checkPars = False Then
            Return New Results.StatusCodeResult(HttpStatusCode.BadRequest, Request)
        End If

        If _parMode Is Nothing Then
            Return New Results.StatusCodeResult(HttpStatusCode.BadRequest, Request)
        End If

        '-------------------------------------------------------------------------------------------------------
        ' SESSION LOG 
        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)

        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)
        '-------------------------------------------------------------------------------------------------------




        ' Kreiraj report 
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' KREIRANJE REPORT FAJLA
        '
        Dim _apiPutniNalog As New NgApiPnPutniNalog()
        Dim _apiReport As New ngApiReport

        Dim _reqFile As String = _apiReport.getRandomFileName(_parPutNalId)
        Dim _destDir As String = ngApiPath.getCurrentPath()


        Dim _signedDoc As Integer
        Dim _pnDoc As Integer = _parPutNalDoc

        Select Case _parMode
            Case "bla"
                _pnDoc = 1
                _signedDoc = 1

            Case "obr"
                _pnDoc = 2
                _signedDoc = 1

            Case Else
                _signedDoc = _pnDoc
        End Select




        Dim _reportCode = "PTNL" + _pnDoc.ToString()
        Dim _dataCode = "PTNL" + _pnDoc.ToString()


        Dim _apiPNalog As New NgApiPnPutniNalog()
        Dim _lastSignedFile As String = _apiPNalog.getLastSignedFile(_parPutNalId, _pnDoc)
        '
        Dim _apiDbFiles As New ngApiDbFiles
        '
        '_reqFile = If(_pnDoc = 1, "in_test_unsigned.pdf", "putnal_105_test.pdf")                                           ' DEVELOPING - COMMENT IN PRODUCTION


        ' uzmi nepotpisani file preko CrystalReports
        '
        _reqFile = _apiReport.createReport(_parPutNalId, _reportCode, _dataCode)                                           ' FOR PRODUCTION - UNCOMMENT IN PRODUCTION
        '_reqFile = "putnal_123_test.pdf"                                                                                  ' FOR DEVELOPING - COMMENT IN PRODUCTION

        Dim _inMemStream As New MemoryStream()
        Dim _outMemStream As New MemoryStream()

        Dim _lastSignedStream = New FileStream(_destDir + _reqFile, IO.FileMode.Open, IO.FileAccess.Read)
        _lastSignedStream.CopyTo(_inMemStream)

        Dim _apiCertificateSign As New ngApiCertificateSign()
        Dim _ApiCertificate As New ngApiCertificate()

        ''''''''' INSERT SLIKA u dokument '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '
        ' Uzima pozicije potpisa i evd_potpisi_id za doc (1 ili 2)
        '
        Dim _signPosCertId As Dictionary(Of Integer, Integer) = _apiPNalog.getSignedPos(_parPutNalId, _signedDoc)
        Dim _offset As Integer = 0

        If _pnDoc <> _signedDoc Then
            _offset = 100
        End If


        _apiCertificateSign.insSignImges2Doc(_signPosCertId, _pnDoc, _inMemStream, _outMemStream, _offset)

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' VRAĆA DATA STREAM fajla: _reportFile
        '
        _reqFile = _apiReport.getRandomFileName(_parPutNalId)
        _apiDbFiles.writeToFile(_reqFile, _destDir, _outMemStream)


        Return Json(_reqFile)

    End Function

    ' API F4 - vraća sadržaj reporta kao BLOB
    <HttpGet>
    <Authorize>
    Public Function GetReportMerge() As IHttpActionResult

        '-------------------------------------------------------------------------------------------------------
        ' Uzima parametre GET
        '
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _pPNId As String = Nothing
        Dim _reportCode = "PTNL10"
        Dim _dataCode = "PTNL1"
        Dim _rptPotpis = True
        Dim _rptWatermark = False

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key.ToLower()
                Case "pPutNalId".ToLower()
                    _pPNId = keypair.Value
                Case "report".ToLower()
                    _reportCode = keypair.Value
                Case "data".ToLower()
                    _dataCode = keypair.Value
                Case "potpis".ToLower()
                    _rptPotpis = IIf(keypair.Value = "true", True, False)
                Case "watermark".ToLower()
                    _rptWatermark = IIf(keypair.Value = "true", True, False)
            End Select
        Next

        If _pPNId Is Nothing Then
            Return New Results.StatusCodeResult(HttpStatusCode.BadRequest, Request)
        End If

        '-------------------------------------------------------------------------------------------------------
        ' SESSION LOG 
        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + ControllerContext.Request.RequestUri.ToString
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)
        '-------------------------------------------------------------------------------------------------------




        ' Kreiraj report 
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' KREIRANJE REPORT FAJLA
        '
        'Dim _apiPutniNalog As New NgApiPnPutniNalog()
        Dim _apiReport As New ngApiReport

        'Dim _reqFile As String = _apiReport.getRandomFileName(_pPNId)





        '
        Dim _apiDbFiles As New ngApiDbFiles


        Dim _pPNIdLst As List(Of String) = _pPNId.Split(",").ToList()

        Dim _reqFile As String = _apiReport.createMergeReport(_pPNIdLst, _reportCode, _dataCode, _rptPotpis, _rptWatermark)
        Dim _destDir As String = ngApiPath.getCurrentPath()





        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' VRAĆA DATA STREAM fajla: _reportFile
        '
        Dim _reqFile2 As String = HttpContext.Current.Server.MapPath("~/Data/" + _reqFile)

        Try
            Dim _dataBytes As Byte() = File.ReadAllBytes(_reqFile2)
            Dim dataStream = New MemoryStream(_dataBytes)
            Return New ngAPIFileResult(dataStream, Request, _reqFile2)
        Catch ex As Exception
            Return New Results.StatusCodeResult(HttpStatusCode.NoContent, Request)
        End Try

    End Function

    ' API F4
    ' GET FILE BY FILENAME
    '
    <HttpGet>
    <Authorize>
    Public Function GetFileContent() As IHttpActionResult
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()


        Dim _reportFile As String = ""


        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "parFilename"
                    _reportFile = keypair.Value
            End Select
        Next


        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)


        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        If _reportFile.Length = 0 Then
            Return New Results.StatusCodeResult(HttpStatusCode.NoContent, Request)
        Else
            Dim _reqFile As String = HttpContext.Current.Server.MapPath("~/Data/" + _reportFile)

            Try
                Dim _dataBytes As Byte() = File.ReadAllBytes(_reqFile)
                Dim dataStream = New MemoryStream(_dataBytes)
                Return New ngAPIFileResult(dataStream, Request, _reportFile)
            Catch ex As Exception
                Return New Results.StatusCodeResult(HttpStatusCode.NoContent, Request)
            End Try

        End If


    End Function


    '
    ' XML Datasource za CR deployment
    '

    <HttpGet>
    Public Function GetReportXML() As IHttpActionResult
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _ReportCode As String = ""
        Dim _DataCode As String = ""
        Dim _XmlName As String = "Unknown"
        Dim _procParam As New List(Of String)

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "parReportCode"
                    _ReportCode = keypair.Value
                Case "parDataCode"
                    _DataCode = keypair.Value
                Case "parXmlName"
                    _XmlName = keypair.Value
                Case Else
                    _procParam.Add(keypair.Value)
                    ' TODO: ovo tretira samo numeric parametre - razviti opciju i za string parametre
            End Select
        Next

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '
        '   LOGGING 
        '
        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)

        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)
        '
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''


        Dim _apiReport As New ngApiReport
        Dim _reqFile = _apiReport.createReportXML(_XmlName, _procParam, _DataCode)


        Try
            Dim _dataBytes As Byte() = File.ReadAllBytes(_reqFile)
            Dim dataStream = New MemoryStream(_dataBytes)
            Return New ngAPIFileResult(dataStream, Request, _XmlName)
        Catch ex As Exception
            Return New Results.StatusCodeResult(HttpStatusCode.NoContent, Request)
        End Try



        'Return Json(_reqFile)

    End Function


    <HttpGet>
    Public Function GetReportJSON() As IHttpActionResult
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _ReportCode As String = ""
        Dim _DataCode As String = ""
        Dim _XmlName As String = "Unknown"
        Dim _procParam As New List(Of String)

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "parDataCode"
                    _DataCode = keypair.Value
                Case Else
                    _procParam.Add(keypair.Value)
                    ' TODO: ovo tretira samo numeric parametre - razviti opciju i za string parametre
            End Select
        Next

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '
        '   LOGGING 
        '
        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)

        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)
        '
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''


        Dim _apiReport As New ngApiReport

        Try
            Dim _reqJSON = _apiReport.createReportJSON(_procParam, _DataCode)
            _reqJSON.Tables(0).TableName = _DataCode
            Return Json(_reqJSON)

        Catch ex As Exception
            Return New Results.StatusCodeResult(HttpStatusCode.NoContent, Request)
        End Try


    End Function


    <Authorize>
    <HttpGet>
    Public Function getReport1() As IHttpActionResult
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _pReportCode As String = ""
        Dim _pDataCode As String = ""
        Dim _pFileName As String = ""
        Dim _pParam As New List(Of String)

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "RCode"
                    _pReportCode = keypair.Value
                Case "DCode"
                    _pDataCode = keypair.Value
                Case "FName"
                    _pFileName = keypair.Value
                Case Else
                    _pParam.Add(keypair.Value)
            End Select
        Next



        '-------------------------------------------------------------------------------------------------------
        ' SESSION LOG 
        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)

        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)
        '-------------------------------------------------------------------------------------------------------

        Dim dataStream As MemoryStream


        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' KREIRANJE REPORT FAJLA
        '

        Dim _apiReport As New ngApiReport

        Dim _reqFile As String = _pFileName
        If _pFileName <> "" Then
            _reqFile = _apiReport.getRandomFileName(_pDataCode + "_" + _pReportCode)
        End If


        '
        _reqFile = _apiReport.createReport(_reqFile, _pReportCode, _pParam, _pDataCode)
        Dim documentPath As String = GetDocumentPath(_reqFile)

        If Not String.IsNullOrEmpty(documentPath) Then

            Dim bytes As Byte() = System.IO.File.ReadAllBytes(documentPath)
            dataStream = New MemoryStream(bytes)
            Return New ngAPIFileResult(dataStream, Request, _reqFile)

        Else
            Return New Results.StatusCodeResult(HttpStatusCode.NotAcceptable, Request)
        End If


    End Function















    '
    ' Syncfusion PDF Datamanager: 
    '                   Load, Bookmarks, RenderPdfPages, RenderThumbnailimages, Unload, Download, PrintImages
    '
    ' API F3-0
    <HttpPost>
    Public Function Load(<FromBody()> ByVal jsonObject As Dictionary(Of String, String)) As IHttpActionResult

        Dim pdfviewer As PdfRenderer = New PdfRenderer()
        Dim stream As MemoryStream = New MemoryStream()
        Dim jsonResult As Object = New Object()

        If jsonObject IsNot Nothing AndAlso jsonObject.ContainsKey("document") Then

            If Boolean.Parse(jsonObject("isFileName")) Then
                Dim documentPath As String = GetDocumentPath(jsonObject("document"))

                If Not String.IsNullOrEmpty(documentPath) Then
                    Dim bytes As Byte() = System.IO.File.ReadAllBytes(documentPath)
                    stream = New MemoryStream(bytes)
                Else
                    Return Json(jsonObject("document") + " is not found")
                End If
            Else
                Dim bytes As Byte() = Convert.FromBase64String(jsonObject("document"))
                stream = New MemoryStream(bytes)
            End If
        End If

        jsonResult = pdfviewer.Load(stream, jsonObject)
        Return Json(jsonResult)

    End Function

    ' API F3-1
    <HttpPost>
    Public Function Bookmarks(<FromBody()> ByVal jsonObject As Dictionary(Of String, String)) As IHttpActionResult

        Dim pdfviewer As PdfRenderer = New PdfRenderer()

        Dim jsonResult As Object = pdfviewer.GetBookmarks(jsonObject)
        Return jsonResult

    End Function

    ' API F3-2
    <HttpPost>
    Public Function RenderPdfPages(<FromBody()> ByVal jsonObject As Dictionary(Of String, String)) As IHttpActionResult

        Dim pdfviewer As PdfRenderer = New PdfRenderer()

        Dim jsonResult As Object = pdfviewer.GetPage(jsonObject)
        Return Json(jsonResult)

    End Function

    ' API F3-3
    <HttpPost>
    Public Function RenderThumbnailImages(<FromBody()> ByVal jsonObject As Dictionary(Of String, String)) As IHttpActionResult

        Dim pdfviewer As PdfRenderer = New PdfRenderer()

        Dim jsonResult As Object = pdfviewer.GetThumbnailImages(jsonObject)
        Return Json(jsonResult)

    End Function

    ' API F3-4
    <HttpPost>
    Public Function Unload(<FromBody()> ByVal jsonObject As Dictionary(Of String, String)) As IHttpActionResult

        Dim pdfviewer As PdfRenderer = New PdfRenderer()

        pdfviewer.ClearCache(jsonObject)
        Return Json("Document cache is cleared")

    End Function

    ' API F3-5
    <HttpPost>
    Public Function Download(<FromBody()> ByVal jsonObject As Dictionary(Of String, String)) As System.Net.Http.HttpResponseMessage

        Dim pdfviewer As PdfRenderer = New PdfRenderer()

        Dim documentBase As String = pdfviewer.GetDocumentAsBase64(jsonObject)
        Return (GetPlainText(documentBase))

    End Function

    ' API F3-6
    '"Unable to cast object of type '<>f__AnonymousType4`2[System.String,System.Collections.Generic.List`1[Syncfusion.EJ2.PdfViewer.PageRenderer+TextMarkupAnnotation]]' 
    ' to type 'System.Net.Http.HttpResponseMessage'.","ExceptionType":"System.InvalidCastException","StackTrace":"   at JwtputnalController.PrintImages(Dictionary`2 jsonObject) 
    'in C:\\inetpub\\wwwroot\\App_Code\\JwtputnalController.vb:line 410\r\n   at lambda_method(Closure , Object , Object[] )\r\n   
    '
    '

    <HttpPost>
    Public Function PrintImages(<FromBody()> ByVal jsonObject As Dictionary(Of String, String)) As Object

        Dim pdfviewer As PdfRenderer = New PdfRenderer()

        Dim pageImage As Object = pdfviewer.GetPrintImage(jsonObject)
        Return (pageImage)

    End Function

    ' API E1
    ''' <summary>
    ''' Simulira Syncfusion ASP.NET MVC DataManager
    ''' </summary>
    ''' <returns></returns>
    <HttpGet>
    <Authorize>
    Public Function Employees() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parSelect As String = ""
        Dim _parInlinecount As String = ""
        Dim _parTop As Integer = 10
        Dim _parFilter As String = ""
        Dim _parByOrgRole As String = ""
        Dim _parExt As String = ""
        Dim _parAkt As String = "*"

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "$select"
                    _parSelect = keypair.Value
                Case "$inlinecount"
                    _parInlinecount = keypair.Value
                Case "$top"
                    _parTop = keypair.Value
                Case "$filter"
                    _parFilter = keypair.Value
                Case "byorgrole"
                    _parByOrgRole = keypair.Value
                Case "ext"
                    _parExt = keypair.Value
                Case "akt"
                    _parAkt = keypair.Value
            End Select
        Next
        'Dim p1Val As String = _allUrlKeyValues.LastOrDefault(Function(x As Dictionary(Of String, String)) x).Value

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _ApiEmployees As New NgApiEmployees
        Dim _ngApiOrganizacija As New NgApiOrganizacija
        Dim unserializedContent As New List(Of NgEmployees)

        Select Case _parByOrgRole
            Case "byorg"


                ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                '
                Dim _roleOrgIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email)
                '
                ' Uzmi rekurzivno sve org id u hijerarhiji organizacije
                _ngApiOrganizacija.GetOrgSifra(_roleOrgIds, 5, 1)

                Dim _orgIds As String = String.Join(",", _ngApiOrganizacija.lstOrgSifras)
                Dim _tmpTable As String = "tmp_" + _ApiSession.EmployeeID.ToString + "_putnalozi"

                'Dim _ApiPutniNalog As New NgPnApiPutniNalog()
                If _orgIds.Length > 0 Then
                    _parFilter = _parFilter.Replace("startswith(tolower(FirstName),'", "")
                    _parFilter = _parFilter.Replace("')", "")

                    _ApiEmployees.Filter = _parFilter
                    _ApiEmployees.Top = _parTop

                    If _parExt.Length = 0 Then
                        unserializedContent = _ApiEmployees.getEmployees(_orgIds, "", _parAkt)
                    Else
                        unserializedContent = _ApiEmployees.getEmployeesExt(_orgIds)
                    End If

                End If


            Case "bypg"

                ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                '
                Dim _rolePGIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email, True)
                '
                _ngApiOrganizacija.GetPGEmpIds(_rolePGIds)
                Dim _PGIds As String = String.Join(",", _ngApiOrganizacija.lstEmpIds)
                Dim _tmpTable As String = "tmp_" + _ApiSession.EmployeeID.ToString + "_putnalozi"

                'Dim _ApiPutniNalog As New NgPnApiPutniNalog()
                If _PGIds.Length > 0 Then
                    _parFilter = _parFilter.Replace("startswith(tolower(FirstName),'", "")
                    _parFilter = _parFilter.Replace("')", "")

                    _ApiEmployees.Filter = _parFilter
                    _ApiEmployees.Top = _parTop

                    If _parExt.Length = 0 Then
                        unserializedContent = _ApiEmployees.getEmployees("", _PGIds, _parAkt)
                    Else
                        unserializedContent = _ApiEmployees.getEmployeesExt("", _PGIds)
                    End If

                End If

            Case "byorgpg"


                ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                '
                Dim _roleOrgIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email)
                '
                ' Uzmi rekurzivno sve org id u hijerarhiji organizacije
                _ngApiOrganizacija.GetOrgSifra(_roleOrgIds, 5, 1)
                Dim _orgIds As String = String.Join(",", _ngApiOrganizacija.lstOrgSifras)

                Dim _rolePGIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email, True)
                _ngApiOrganizacija.GetPGEmpIds(_rolePGIds)
                Dim _PGIds As String = String.Join(",", _ngApiOrganizacija.lstEmpIds)

                Dim _tmpTable As String = "tmp_" + _ApiSession.EmployeeID.ToString + "_putnalozi"

                'Dim _ApiPutniNalog As New NgPnApiPutniNalog()
                If _orgIds.Length > 0 Or _PGIds.Length > 0 Then
                    _parFilter = _parFilter.Replace("startswith(tolower(FirstName),'", "")
                    _parFilter = _parFilter.Replace("')", "")

                    _ApiEmployees.Filter = _parFilter
                    _ApiEmployees.Top = _parTop

                    If _parExt.Length = 0 Then
                        unserializedContent = _ApiEmployees.getEmployees(_orgIds, _PGIds, _parAkt)
                    Else
                        unserializedContent = _ApiEmployees.getEmployeesExt(_orgIds, _PGIds)
                    End If

                End If

            Case Else
                _parFilter = _parFilter.Replace("startswith(tolower(FirstName),'", "")
                _parFilter = _parFilter.Replace("')", "")

                _ApiEmployees.Filter = _parFilter
                _ApiEmployees.Top = _parTop
                unserializedContent = _ApiEmployees.getEmployees()

        End Select




        'unserializedContent = _ApiEmployees.getEmployees()
        Return Json(unserializedContent)

    End Function

    Private Function GetPlainText(ByVal pageImage As String) As HttpResponseMessage
        Dim responseText = New HttpResponseMessage(HttpStatusCode.OK)
        responseText.Content = New StringContent(pageImage, System.Text.Encoding.UTF8, "text/plain")
        Return responseText
    End Function

    ' API eS1
    '<HttpPost>
    '<Authorize>
    'Public Function PutNalSign(<FromBody> ByVal pESignature As NgESignature) As IHttpActionResult

    '    Dim _ApiSession As New NgApiSession
    '    _ApiSession.getClaims(Thread.CurrentPrincipal)
    '    ' Provjere:
    '    '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
    '    '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
    '    '  3 - Da li pESignature.pEmployeeID odgovara EmployeeID usera
    '    '      Ako NE, vratiti FALSE
    '    '  
    '    Dim _className As String = Me.GetType().Name
    '    Dim _methodName As String = MethodBase.GetCurrentMethod().Name

    '    Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
    '    Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
    '    _ApiSession.insApiLog(_log, _comment)


    '    Dim _ApiPutniNalog As New NgApiPnPutniNalog()
    '    '
    '    '
    '    ' Uzmi putni nalog
    '    Dim _pn As ngPnPutniNalog = _ApiPutniNalog.getPnPutniNalogById(pESignature.pPutNalID)
    '    ' Postavi pEmpID koji pripada userusesije 
    '    pESignature.pEmployeeID = _ApiSession.EmployeeID
    '    '
    '    '
    '    ' Provjeri i upiši u DB potpis
    '    Dim _replyMess = _ApiPutniNalog.signPutNal(pESignature, _pn, True)

    '    '
    '    '
    '    ' Promjeni status naloga, ako je potpisnik ovlašten
    '    If _replyMess = True Then
    '        ' API eS"
    '        Dim _signStatus As String = _ApiPutniNalog.signStatusToPutNal(pESignature, _pn)

    '        If _signStatus.Length > 0 Then
    '            _ApiPutniNalog.updPutNalStatus(pESignature.pPutNalID, _signStatus, _ApiSession.EmployeeID)
    '        End If

    '        'Return Json(True)
    '    Else
    '        'Return Json(False)
    '    End If

    '    Return Json(_replyMess)

    'End Function

    ' API eD1
    '<HttpPost>
    '<Authorize>
    'Public Function PutNalUnSign(<FromBody> ByVal pESignature As NgESignature) As IHttpActionResult

    '    Dim _ApiSession As New NgApiSession
    '    _ApiSession.getClaims(Thread.CurrentPrincipal)
    '    ' Provjere:
    '    '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
    '    '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
    '    '  3 - Da li pESignature.pEmployeeID odgovara EmployeeID usera
    '    '      Ako NE, vratiti FALSE
    '    '  4 - Obrisati potpis, samo ako je identičan potpisu pESignature.pPutNalESignature
    '    '  5 - Ako nema više nijednog potpisa, promijeniti status naloga u OTVOREN
    '    '  
    '    Dim _className As String = Me.GetType().Name
    '    Dim _methodName As String = MethodBase.GetCurrentMethod().Name

    '    Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
    '    Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
    '    _ApiSession.insApiLog(_log, _comment)



    '    Dim _ApiPutniNalog As New NgApiPnPutniNalog()
    '    '
    '    ' Uzmi evd_potpisi_id
    '    Dim _eSignature As New NgESignature
    '    _eSignature.pEmployeeID = _ApiSession.EmployeeID
    '    _eSignature = _ApiPutniNalog.listPublicKey(_eSignature)

    '    ' Obriši prema pnid i evd_potpisi_id
    '    '
    '    Dim _replyMess = _ApiPutniNalog.delPutNalESignature(pESignature.pPutNalID, _eSignature.ePublicKeyId)

    '    '
    '    '
    '    ' Promjeni status naloga, ako je potpisnik ovlašten
    '    If _replyMess = True Then

    '        pESignature.pEmployeeID = _ApiSession.EmployeeID
    '        ' Uzmi status putnog naloga
    '        Dim _pn As ngPnPutniNalog = _ApiPutniNalog.getPnPutniNalogById(pESignature.pPutNalID)
    '        Dim _signStatus As String = _ApiPutniNalog.signStatusFromPutNal(pESignature, _pn)

    '        If _signStatus.Length > 0 Then
    '            _ApiPutniNalog.updPutNalStatus(pESignature.pPutNalID, _signStatus, _ApiSession.EmployeeID)
    '        End If

    '        'Return Json(True)
    '    Else
    '        'Return Json(False)
    '    End If

    '    Return Json(_replyMess)

    'End Function


    ' API esL1 - Vraća ESignature objekt sa PublicKey ako postoji 
    <HttpPost>
    <Authorize>
    Public Function ESignList(<FromBody> ByVal pESignature As NgESignature) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  ...
        '  ...
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _ApiPutniNalog As New NgApiPnPutniNalog()
        Dim _reply = _ApiPutniNalog.listESignature(pESignature) ' uzima PublicKey i PublicKeyId i smješta u pESignature - vraća True ako PublicKey postoji

        Dim unserializedContent As New NgESignature
        If _reply = True Then
            unserializedContent = pESignature
            Return Json(unserializedContent)
        Else
            unserializedContent.pEmployeeID = pESignature.pEmployeeID
        End If



        Return Json(unserializedContent)

    End Function

    ' API esC1 - insertuje PublicKey u bazu
    <HttpPost>
    <Authorize>
    Public Function ESignIns(<FromBody> ByVal pESignature As NgESignature) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _ApiPutniNalog As New NgApiPnPutniNalog()
        Dim _reply = _ApiPutniNalog.insESignature(pESignature)  ' insertuje PublicKey u bazu

        Dim unserializedContent As New NgESignature
        If _reply = True Then
            unserializedContent = pESignature
            Return Json(unserializedContent)
        Else
            unserializedContent.pEmployeeID = pESignature.pEmployeeID
        End If



        Return Json(unserializedContent)

    End Function


    ' API esD1
    <HttpPost>
    <Authorize>
    Public Function ESignDel(<FromBody> ByVal pESignature As NgESignature) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name




        If _ApiSession.IsApiRoleAllowed(_className, _methodName) Then

            Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
            Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
            _ApiSession.insApiLog(_log, _comment)


            Dim _ApiPutniNalog As New NgApiPnPutniNalog()
            Dim _reply = _ApiPutniNalog.delESignature(pESignature)

            Return Json(_reply)

        Else

            Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
        End If




    End Function


    ' API aRD     - Insert row in evd_potpisi, Ready for PublicKey
    '<EnableCors("http://localhost:56300", "*", "*")>
    <HttpGet>
    <Authorize>
    Public Function ESignReady() As IHttpActionResult
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parEmployeeId As Integer = -1


        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "pEmployeeId"
                    If Not (Integer.TryParse(keypair.Value, _parEmployeeId) = True) Then
                        _parEmployeeId = -1
                    End If
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + ": " + _className + "." + _methodName
        Dim _comment As String = "Ready for PublicKey " + _parEmployeeId.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _retValue As Boolean = False

        If (_ApiSession.IsApiRoleAllowed(_className, _methodName)) AndAlso (_parEmployeeId > 0) Then

            Dim _ApiPutniNalog As New NgApiPnPutniNalog()
            Dim _reply = _ApiPutniNalog.readyESignature(_parEmployeeId)

            _comment = "Ready for PublicKey successfully inserted: " + _parEmployeeId.ToString
            _ApiSession.insApiLog(_log, _comment)
            Return Json(_reply)
        Else

            Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
        End If


    End Function

    ' API esPK
    <HttpPost>
    <Authorize>
    Public Function ESignKey(<FromBody> ByVal pESignature As NgESignature) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _NgESignature As New NgESignature

        Select Case _ApiSession.Role
            Case "uposlenik"
                _NgESignature.pEmployeeID = _ApiSession.EmployeeID
                _NgESignature.ePublicKey = pESignature.ePublicKey

            Case "administrator"

        End Select




        Dim _ApiPutniNalog As New NgApiPnPutniNalog()
        Dim _reply = _ApiPutniNalog.listPublicKey(_NgESignature)



        Return Json(_reply)

    End Function

    ' API aAK     - Insert row in evd_potpisi_status, pravi potpis
    '<EnableCors("http://localhost:56300", "*", "*")>
    <HttpGet>
    <Authorize>
    Public Function ESignAuthorize() As IHttpActionResult
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parEmployeeId As Integer = -1
        Dim _parDocId As Integer = -1
        Dim _parStatusFrom As String = ""
        Dim _parStatusTo As String = ""

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "pEmployeeId"
                    If Not (Integer.TryParse(keypair.Value, _parEmployeeId) = True) Then
                        _parEmployeeId = -1
                    End If
                Case "pDocId"
                    If Not (Integer.TryParse(keypair.Value, _parDocId) = True) Then
                        _parDocId = -1
                    End If
                Case "pStatusFrom"
                    _parStatusFrom = keypair.Value
                Case "pStatusTo"
                    _parStatusFrom = keypair.Value
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + ": " + _className + "." + _methodName
        Dim _comment As String = "Authorize for PublicKey " + _parEmployeeId.ToString

        _ApiSession.insApiLog(_log, _comment)

        Dim _retValue As Boolean = False

        If (_ApiSession.IsApiRoleAllowed(_className, _methodName)) AndAlso (_parEmployeeId > 0) Then
            Dim _ApiPutniNalog As New NgApiPnPutniNalog()

            Dim pESignature As New NgESignature()
            pESignature.pEmployeeID = _parEmployeeId

            Dim _replyEmpId = _ApiPutniNalog.listESignature(pESignature)
            If _replyEmpId = True Then

                Dim _reply = _ApiPutniNalog.authorizeESignature(pESignature.ePublicKeyId)

                _comment = "Epotpisinik successfully inserted: " + _parEmployeeId.ToString
                _ApiSession.insApiLog(_log, _comment)
                Return Json(_reply)
            Else
                Return New Results.StatusCodeResult(HttpStatusCode.NotFound, Request)
            End If

        Else

            Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
        End If


    End Function

    ' API aAD     - Insert row in evd_potpisi_status, pravi potpis
    '<EnableCors("http://localhost:56300", "*", "*")>
    <HttpGet>
    <Authorize>
    Public Function ESignDelAuthorize() As IHttpActionResult
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parEmployeeId As Integer = -1
        Dim _parDocId As Integer = -1
        Dim _parStatusFrom As String = ""

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "pEmployeeId"
                    If Not (Integer.TryParse(keypair.Value, _parEmployeeId) = True) Then
                        _parEmployeeId = -1
                    End If
                Case "pDocId"
                    If Not (Integer.TryParse(keypair.Value, _parDocId) = True) Then
                        _parDocId = -1
                    End If
                Case "pStatusFrom"
                    _parStatusFrom = keypair.Value
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + ": " + _className + "." + _methodName
        Dim _comment As String = "Delete Authorize for PublicKey " + _parEmployeeId.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _retValue As Boolean = False

        If (_ApiSession.IsApiRoleAllowed(_className, _methodName)) AndAlso (_parEmployeeId > 0) Then
            Dim _ApiPutniNalog As New NgApiPnPutniNalog()

            Dim pESignature As New NgESignature()
            pESignature.pEmployeeID = _parEmployeeId

            Dim _replyEmpId = _ApiPutniNalog.listESignature(pESignature)
            If _replyEmpId = True Then

                Dim _reply = _ApiPutniNalog.delAuthorizeESignature(pESignature.ePublicKeyId)


                _comment = "Epotpisnik successfully deleted: " + _parEmployeeId.ToString
                _ApiSession.insApiLog(_log, _comment)
                Return Json(_reply)
            Else
                Return New Results.StatusCodeResult(HttpStatusCode.NotFound, Request)
            End If

        Else

            Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
        End If


    End Function

    ' API aAL     - Lista dokumenata sa promjenama statusa
    '<EnableCors("http://localhost:56300", "*", "*")>
    <HttpGet>
    <Authorize>
    Public Function ESignAuthorizeList() As IHttpActionResult
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parEmployeeId As Integer = -1
        Dim _parDocId As Integer = -1
        Dim _parStatusFrom As String = ""

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "pEmployeeId"
                    If Not (Integer.TryParse(keypair.Value, _parEmployeeId) = True) Then
                        _parEmployeeId = -1
                    End If
                Case "pDocId"
                    If Not (Integer.TryParse(keypair.Value, _parDocId) = True) Then
                        _parDocId = -1
                    End If
                Case "pStatusFrom"
                    _parStatusFrom = keypair.Value
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + ": " + _className + "." + _methodName
        Dim _comment As String = "List Authorize status with documents " + _parEmployeeId.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _ApiPutniNalog As New NgApiPnPutniNalog()

        Dim pESignature As New NgESignature()
        pESignature.pEmployeeID = _parEmployeeId

        Dim _replyEmpId = _ApiPutniNalog.listESignature(pESignature) ' PROVJERAVA da li postoji potpis
        If _replyEmpId = True Then

            Dim _reply = _ApiPutniNalog.delAuthorizeESignature(pESignature.ePublicKeyId)


            _comment = "Epotpisnik successfully deleted: " + _parEmployeeId.ToString
            _ApiSession.insApiLog(_log, _comment)
            Return Json(_reply)
        Else
            Return New Results.StatusCodeResult(HttpStatusCode.NotFound, Request)
        End If


    End Function

    ' API esUL1     - List of employees having esignature
    <HttpGet>
    <Authorize>
    Public Function ESignEmployeeList() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parList As String = ""

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "list"
                    _parList = keypair.Value
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _ngApiOrganizacija As New NgApiOrganizacija

        Dim _tmpTable As String = "tmp_" + _ApiSession.EmployeeID.ToString + "_apissesion"
        Dim unserializedContent As Object

        Dim _ApiPutniNalog As New NgApiPnPutniNalog()

        If _ApiSession.IsApiRoleAllowed(_className, _methodName) Then
            Select Case _parList
                Case "byorg"


                    ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                    '
                    Dim _roleOrgIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email)
                    '
                    ' Uzmi rekurzivno sve org id u hijerarhiji organizacije
                    _ngApiOrganizacija.GetOrgSifra(_roleOrgIds, 5, 1)

                    Dim _orgIds As String = String.Join(",", _ngApiOrganizacija.lstOrgSifras)


                    If _orgIds.Length > 0 Then
                        unserializedContent = _ApiPutniNalog.getEsignEmployeeList(_orgIds, "", _tmpTable, _ApiSession.EmployeeID)
                        Return Json(unserializedContent)
                    Else

                        Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
                    End If


                Case "bypg"

                    ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                    '
                    Dim _rolePGIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email, True)
                    '
                    _ngApiOrganizacija.GetPGEmpIds(_rolePGIds)
                    Dim _PGIds As String = String.Join(",", _ngApiOrganizacija.lstEmpIds)

                    If _PGIds.Length > 0 Then
                        unserializedContent = _ApiPutniNalog.getEsignEmployeeList("", _PGIds, _tmpTable, _ApiSession.EmployeeID)
                        Return Json(unserializedContent)
                    Else

                        Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
                    End If

                Case "byorgpg"


                    ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                    '
                    Dim _roleOrgIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email)

                    '
                    ' Uzmi rekurzivno sve org id u hijerarhiji organizacije
                    _ngApiOrganizacija.GetOrgSifra(_roleOrgIds, 5, 1)
                    Dim _orgIds As String = String.Join(",", _ngApiOrganizacija.lstOrgSifras)

                    Dim _rolePGIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email, True)
                    _ngApiOrganizacija.GetPGEmpIds(_rolePGIds)
                    Dim _PGIds As String = String.Join(",", _ngApiOrganizacija.lstEmpIds)


                    If _orgIds.Length > 0 Or _PGIds.Length > 0 Then
                        unserializedContent = _ApiPutniNalog.getEsignEmployeeList(_orgIds, _PGIds, _tmpTable, _ApiSession.EmployeeID)
                        Return Json(unserializedContent)
                    Else

                        Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
                    End If


            End Select
        Else

            Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
        End If

        Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
    End Function

    '----------------------------------------------------------'
    ' API: ECertifikat pfx                                     '
    '----------------------------------------------------------'

    ' API ecC2 - insertuje X509 certifikat u sql bazu
    <HttpPost>
    <Authorize>
    Public Function ECertIns(<FromBody> ByVal pECertificate As NgECertificate) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)

        ' Insert LOG 
        '
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + Me.GetType().Name + "." + MethodBase.GetCurrentMethod().Name
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)



        Dim unserializedContent As Object = False

        '
        ' Nema certifikat, upiši novi
        '
        ' Postavi parametre certifikata...
        '
        Dim _ApiReport As New ngApiReport
        Dim destDir As String = ngApiPath.getCurrentPath()

        Dim _ECertificate As New NgECertificate

        _ECertificate.employeeID = pECertificate.employeeID
        _ECertificate.subjectCN = pECertificate.subjectCN   ' TODO:  preuzeti iz baze ***************** 1
        _ECertificate.issuerCN = pECertificate.issuerCN     ' TODO:  preuzeti iz baze ***************** 2

        _ECertificate.cType = CType([Enum].Parse(GetType(ContainerType), "PKCS12"), ContainerType) '' nude se opcije: PKCS12 ili DER ... enum ContainerType
        _ECertificate.hType = CType([Enum].Parse(GetType(HashType), "SHA256withRSA"), HashType)    '' hash type ... enum HashType
        _ECertificate.bitStrength = CInt(2048)
        _ECertificate.validFrom = New DateTime(Now().Year, Now().Month, Now().Day, Now().Hour, Now().Minute, Now().Second)
        _ECertificate.validTo = New DateTime(Now().Year + pECertificate.validYears, Now().Month, Now().Day, Now().Hour, Now().Minute, Now().Second)

        _ECertificate.X509Name_C = "BA"                     ' TODO:  preuzeti iz baze ***************** 3
        _ECertificate.password = pECertificate.password


        '_ECertificate.X509Name_O = pECertificate.X509Name_O ' TODO: ime i prezime preizeti iz baze: BHRT Jasminko Bešić


        Dim _ngUser As NgUser = _ApiSession.getEmployeeData(_ApiSession.EmployeeID)
        ' User uopšte nema potpisa
        If (_ngUser Is Nothing) Then
            Return New Results.StatusCodeResult(HttpStatusCode.PreconditionFailed, Request)
        End If

        '_ECertificate.X509Name_O = _ngUser.OrgJedNaziv
        _ECertificate.X509Name_O = _ngUser.LastName + " " + _ngUser.FirstName + " (" + _ngUser.EmployeeID.ToString + ")"


        _ECertificate.X509Name_L = pECertificate.X509Name_L     ' TODO:  preuzeti iz baze ***************** 4
        _ECertificate.X509Name_ST = pECertificate.X509Name_ST   ' TODO:  preuzeti iz baze ***************** 5
        _ECertificate.X509Name_E = pECertificate.X509Name_E     ' TODO:  preuzeti iz baze ***************** 6


        Dim _ApiCertificate As New ngApiCertificate()
        Dim _ApiDbFiles As New ngApiDbFiles

        ' Provjeri da li već ima aktivan ECertifikat ili je deaktiviran
        ' Ovdje uzima id ecert iz tabele: _ECertificate.id

        Dim _isReady = _ApiCertificate.isDbReady(_ECertificate)

        _comment = "Ready for Certificate: " + pECertificate.employeeID.ToString
        _ApiSession.insApiLog(_log, _comment)


        '
        ' Generiši certifikat 
        '
        If _isReady Then
            _ApiCertificate.GenerateCertificate(_ECertificate)
            ''
            '' i spasi u sql bazu (ili file sistem)
            ''
            '' uzmi i PUBLICKEY 
            Dim _certX509 = _ApiCertificate.certX509(_ECertificate)         ' Bytes() je već napunjen metodom GenerateCertificate

            unserializedContent = _ApiDbFiles.writePotpisPfx2Db(_ECertificate.id, _ApiCertificate.rawData, _ApiCertificate.publicKey)
            'unserializedContent = _ApiCertificate.Save(destDir + "tst")

        End If

        _ApiCertificate.getStatus(_ECertificate)
        _ApiCertificate.getOdobrenje(_ECertificate) ' Dodaje i status odobrenja

        unserializedContent = _ECertificate
        Return Json(unserializedContent)


    End Function

    ' API ecL1 - lista X509 certifikat status
    <HttpPost>
    <Authorize>
    Public Function ECertList(<FromBody> ByVal pECertificate As NgECertificate) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)

        ' Insert LOG 
        '
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + Me.GetType().Name + "." + MethodBase.GetCurrentMethod().Name
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)



        Dim unserializedContent As Object = False

        '
        ' Nema certifikat, upiši novi
        '
        ' Postavi parametre certifikata...
        '
        Dim _ApiReport As New ngApiReport
        Dim destDir As String = ngApiPath.getCurrentPath()

        Dim _ECertificate As New NgECertificate

        _ECertificate.employeeID = pECertificate.employeeID


        Dim _ApiCertificate As New ngApiCertificate()
        Dim _ApiDbFiles As New ngApiDbFiles

        ' Uzmi status certifikata u _ECertificate
        '

        _ApiCertificate.getStatus(_ECertificate)
        _ApiCertificate.getOdobrenje(_ECertificate) ' Dodaje i status odobrenja

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' LOGGER
        _comment = "Ready for Certificate: " + pECertificate.employeeID.ToString
        _ApiSession.insApiLog(_log, _comment)
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

        unserializedContent = _ECertificate
        Return Json(unserializedContent)


    End Function

    ' API ecL1 - provjeri password X509 certifikata
    <HttpPost>
    <Authorize>
    Public Function ECertVerify(<FromBody> ByVal pECertificate As NgECertificate) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)

        ' Insert LOG 
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + Me.GetType().Name + "." + MethodBase.GetCurrentMethod().Name
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _ApiCertificate As New ngApiCertificate()
        Dim _ApiDbFiles As New ngApiDbFiles

        ' Provjeri da li je password u redu ...
        Dim _isReady = _ApiCertificate.VerifyPassword(pECertificate)

        ' Insert LOG 
        _comment = "Ready for Certificate: " + pECertificate.employeeID.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim unserializedContent As Object = False
        unserializedContent = _isReady
        Return Json(unserializedContent)


    End Function


    ' API ecC1 - insertuje sliku potpisa u sql bazu
    <HttpPost>
    <Authorize>
    Public Function ECertImgIns() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()
        Dim _parEmpID As Integer = -1

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "pEmployeeID"
                    If Not (Integer.TryParse(keypair.Value, _parEmpID) = True) Then
                        _parEmpID = -2
                    End If
            End Select
        Next


        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)

        ' Insert LOG 
        '
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + Me.GetType().Name + "." + MethodBase.GetCurrentMethod().Name
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        If Not Request.Content.IsMimeMultipartContent() Then Throw New HttpResponseException(HttpStatusCode.UnsupportedMediaType)

        '' 
        ''
        ''
        Dim unserializedContent As Object = False

        Dim _ApiCertificate As New ngApiCertificate()
        Dim _ApiDbFiles As New ngApiDbFiles

        Dim _ECertificate As New NgECertificate
        _ECertificate.employeeID = _parEmpID

        Dim httpRequest = HttpContext.Current.Request

        For Each file As String In httpRequest.Files
            Dim postedFile = httpRequest.Files(file)

            Dim _fs As Stream = postedFile.InputStream
            Dim _br As BinaryReader = New BinaryReader(_fs)
            'Dim _bytes As Byte() = _br.ReadBytes(CType(_fs.Length, Int32))
            _ApiCertificate.rawData = _br.ReadBytes(CType(_fs.Length, Int32))

        Next

        ' Provjeri da li već ima aktivan red u sql bazi za sliku
        '
        Dim _isReady = _ApiCertificate.isDbReady(_ECertificate)

        '
        ' Upiši sliku u bazu
        '
        If Not _isReady AndAlso _ECertificate.ecertStatus = "no_image" Then
            ''
            '' Spasi u sql bazu (ili file sistem)
            ''
            Dim _rowsAffected As Integer = _ApiDbFiles.writePotpisImg2Db(_ECertificate.id, _ApiCertificate.rawData)
            If _rowsAffected > 0 Then unserializedContent = True
        End If

        If unserializedContent <> True Then Throw New HttpResponseException(HttpStatusCode.NotAcceptable)

        Return Json(unserializedContent)


    End Function

    ' API ecD1 - briše sliku potpisa iz db
    <HttpGet>
    <Authorize>
    Public Function ECertImgDel() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()
        Dim _parEmpID As Integer = -1

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "pEmployeeID"
                    If Not (Integer.TryParse(keypair.Value, _parEmpID) = True) Then
                        _parEmpID = -2
                    End If
            End Select
        Next


        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)

        ' Insert LOG 
        '
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + Me.GetType().Name + "." + MethodBase.GetCurrentMethod().Name
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        ''
        ''
        Dim unserializedContent As Object = False

        Dim _ApiCertificate As New ngApiCertificate()
        Dim _ApiDbFiles As New ngApiDbFiles

        Dim _ECertificate As New NgECertificate
        _ECertificate.employeeID = _parEmpID


        ' Provjeri da li već ima aktivan red u sql bazi za sliku i da li je img size > 0, ako jeste true
        Dim _isReady = _ApiCertificate.isDbReady(_ECertificate)

        '
        ' Upiši NULL za sliku u db
        '
        If _isReady AndAlso _ECertificate.ecertStatus = "ready for certificate" Then
            ''
            '' Set to NULL img 
            Dim _rowsAffected As Integer = _ApiDbFiles.removePotpisImg2Db(_ECertificate.id)
            If _rowsAffected > 0 Then unserializedContent = True
        End If

        If unserializedContent <> True Then Throw New HttpResponseException(HttpStatusCode.NotAcceptable)

        Return Json(unserializedContent)


    End Function

    ' API ecL2 - uzima sliku potpisa
    <HttpGet>
    <Authorize>
    Public Function ECertImgGet() As IHttpActionResult
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()
        Dim _parEmpID As Integer = -1

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "pEmployeeID"
                    If Not (Integer.TryParse(keypair.Value, _parEmpID) = True) Then
                        _parEmpID = -2
                    End If
            End Select
        Next

        '-------------------------------------------------------------------------------------------------------
        ' SESSION LOG 
        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)
        '-------------------------------------------------------------------------------------------------------

        Dim _ApiCertificate As New ngApiCertificate()
        Dim _ApiDbFiles As New ngApiDbFiles
        Dim _ECertificate As New NgECertificate
        _ECertificate.employeeID = _parEmpID

        ' Uzmi id certifikata
        Dim _isReady = _ApiCertificate.isDbReady(_ECertificate)

        ' Pročitaj img iz baze, i pretvori u MemoryStream
        Dim cRawData() As Byte = _ApiDbFiles.readPotpisImgFromDb(_ECertificate.id)
        Dim dataStream As MemoryStream = New MemoryStream(cRawData)

        ' Daj naziv fajlu
        Dim _reportFile = String.Format("img_ecert_{0}.png", _parEmpID)

        Return New ngAPIFileResult(dataStream, Request, _reportFile)

    End Function


    '                                                               '
    ' E SIGNATURE PUTNI NALOG - BY CERTIFICATE pfx                  '
    '                                                               '

    ' API esC2 - potpisuje pdf certifikatom pfx
    <HttpPost>
    <Authorize>
    Public Function PutNalSignByCert(<FromBody> ByVal pInData As ngPnPotpisByCert) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role
        Dim _comment As String = "Api: " + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)




        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' PROVJERA PRAVA NA POTPIS
        '
        Dim _ApiPNalog As New NgApiPnPutniNalog()
        Dim _ApiCertSign As New ngApiCertificateSign()
        Dim _ApiCertificate As New ngApiCertificate()

        ' PRAVO NA POTPIS OK - INIT

        Dim _ECert As New NgECertificate
        _ECert.employeeID = _ApiSession.EmployeeID              ' Potpis se može davati samo iz svoje sesije !!!
        _ECert.password = pInData.password

        Dim _isReady = _ApiCertificate.isDbReady(_ECert)        ' provjera da li ima pfx certifikat ?? SAMO PROVJERAVA DA LI IMA SLIKU POTPISA
        If (_isReady = False) Then
            Return New Results.StatusCodeResult(HttpStatusCode.NotAcceptable, Request)
        End If

        ' Provjera da li je dokument već potpisan od signer-a: 0-No, 1-Yes
        Dim _isSignedBySigner As Integer = _ApiPNalog.isSignedBySigner(pInData.pnID, pInData.pnDoc, _ECert.id)
        If (_isSignedBySigner > 0) Then
            Return New Results.StatusCodeResult(HttpStatusCode.NotAcceptable, Request)
        End If

        ' *** uzimanje POZICIJE POTPISA iz json i max broja pozicija potpisa
        '
        _ApiCertSign.setSignPosition(pInData.pnDoc, pInData.signPos)

        ' Provjera ukupnog mogućeg broja potpisa na dokumentu
        If (pInData.signPos > _ApiCertSign.MaxPos) Then
            Return New Results.StatusCodeResult(HttpStatusCode.NotAcceptable, Request)
        End If

        ' Provjera da li je na toj poziciji već upisan potpis: 0-No, 1-Yes
        Dim _isSignedByPos As Integer = _ApiPNalog.isSignedByPos(pInData.pnID, pInData.pnDoc, pInData.signPos)

        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' IZUZETAK: ako se traži pInData.signPos = 7, provjeriti da li je pozicija 4 aktivna. Ako jeste, vratiti 1

        If pInData.signPos = 7 AndAlso _isSignedByPos = 0 Then
            Dim _signPos4 As Integer = 4
            _isSignedByPos = _ApiPNalog.isSignedByPos(pInData.pnID, pInData.pnDoc, _signPos4)
        End If
        '
        '
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''


        If _isSignedByPos > 0 Then
            Return New Results.StatusCodeResult(HttpStatusCode.NotAcceptable, Request)
        End If


        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' E-POTPIS PODATAKA: upisuje u tabelu putninalog_potpisi
        '
        Dim pESignature As New NgESignature
        pESignature.pPutNalID = pInData.pnID
        pESignature.pEmployeeID = _ApiSession.EmployeeID    ' Postavi pEmpID koji pripada userusesije 
        pESignature.pPutNalESignature = "01234567890"       ' Ne postoji ESignature kao kod 1 vrste potpisa, ovo je Dummy

        ' Uzmi putni nalog
        Dim _pn As ngPnPutniNalog = _ApiPNalog.getPnPotpis_1_ById(pInData.pnID)

        'TODO: _pnJSON treba da bude zavisno o kojoj se vrsti dokumenta radi: 1 ili 2

        Dim _pnJSON As String = JsonConvert.SerializeObject(_pn)
        '
        ' Provjeri i upiši u DB potpis
        Dim _replyMess = _ApiPNalog.signPutNalByCert2(pESignature, _pnJSON, _ECert, pInData, True)

        '
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' UZIMANJE PDF NALOGA
        '
        ' Uzmi poziciju i naziv zadnjeg potpisanog pdf dokumenta
        '
        Dim _lastSignedFile As String = _ApiPNalog.getLastSignedFile(pInData.pnID, pInData.pnDoc)       ' puni path (sa  HttpContext) i naziv fajla

        Dim _apiDbFiles As New ngApiDbFiles

        Dim _outMemStream As New MemoryStream()
        Dim _InMemStream As MemoryStream = New MemoryStream()

        Dim _fileName As String = _apiDbFiles.getRandomFileName(pInData.pnID, pInData.pnDoc)            ' formira naziv fajla
        Dim _filePath As String = "~/Data/PutNal/"                                                             ' path fajla
        Dim _fileDestDir As String = HttpContext.Current.Server.MapPath(_filePath)

        'Dim _rptFile As String = If(pInData.pnDoc = 1, "putnal_69_input.pdf", "putnal_69_input21.pdf")                                                               ' DEVELOPING - COMMENT
        Dim _rptCode As String = If(pInData.pnDoc = 1, "PTNL1", "PTNL2")

        Select Case _lastSignedFile
            Case ""  ' uzmi nepotpisani file preko CrystalReports
                Dim _apiReport As New ngApiReport
                '
                ' KREIRANJE REPORTA KAO FILE STREAM UMJESTO FILE !!! NA CLOUD SERVERU IZBACUJE GREŠKU

                '''' !!!
                Dim destDir As String = ngApiPath.getCurrentPath()

                Dim _rptFile As String = _apiReport.createReport(pInData.pnID, _rptCode, _rptCode)                                                                     ' PRODUCTION - UNCOMMENT
                Dim _rptFileStream = New FileStream(destDir + _rptFile, IO.FileMode.Open, IO.FileAccess.Read)                                                          ' PRODUCTION - UNCOMMENT

                'Dim _rptFile As String = "putnal_142_ubple5nut4r.pdf"                                                                                                   ' DEVELOPING - COMMENT
                'Dim _rptFileStream = New FileStream(HttpContext.Current.Server.MapPath("~/Data/PutNal/") + _rptFile, IO.FileMode.Open, IO.FileAccess.Read)              ' DEVELOPING - COMMENT
                _rptFileStream.CopyTo(_InMemStream)


                ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
                '   INSERT SLIKE POTPISA NA POZICIJE NALOGODAVCA - OBRAČUN NALOGA
                '   pInData.pnDoc = 2
                '                   (1) uzmi pozicije potpisa i NgECertificate.id _ECert.id radi uzimanja slike 
                '                       ngApiCertificateSign(.getImage(_ECert.id)
                '                   (2) upiši na ekvivalentne pozicije potpisa za pnDoc = 2: id +100 (101, 102, ...)


                If pInData.pnDoc = 2 Then
                    Dim __ApiCertSign As New ngApiCertificateSign()

                    Dim _signedDoc As Integer = 1                                                                               ' uzmi potpise iz pn za putovanje 
                    Dim _signPosCertId As Dictionary(Of Integer, Integer) = _ApiPNalog.getSignedPos(pInData.pnID, _signedDoc)   ' uzmi pozicije potpisa iz tog naloga

                    Dim _offset As Integer = 100
                    __ApiCertSign.insSignImges2Doc(_signPosCertId, pInData.pnDoc, _InMemStream, _outMemStream, _offset)
                End If

                '
                ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

            Case Else  ' uzmi potpisani file iz putninalog_doc_documents 

                '''' Uzmi pdf iz FILE SISTEMA !!!
                Dim _lastSignedStream = New FileStream(_lastSignedFile, IO.FileMode.Open, IO.FileAccess.Read)
                _lastSignedStream.CopyTo(_InMemStream)

                Dim _isCorrupted1 = _ApiCertSign.analyzePdfDocument(_InMemStream).IsCorrupted
                If _isCorrupted1 Then

                    _log = "putnalsignbycert - last signed file : HTTP status 409"
                    _comment = "FAILED: pdf corrupted: " + _lastSignedFile
                    _ApiSession.insApiLog(_log, _comment)

                    Return New Results.StatusCodeResult(HttpStatusCode.Conflict, Request)
                End If

        End Select
        '
        '
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' E-POTPIS PDF NALOGA I SPAŠAVANJE
        '
        ' Ako su pozicije potpisa definisane markerom preuzmi 
        If _ApiCertSign.MarkerPos <> "" Then
            _ApiCertSign.getMarkerPosition(_InMemStream)
        End If

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '
        '   INSERT SLIKE POTPISA NA IZVJEŠTAJ - pozicija 7 (ako ova pozicija nije potpisana)


        ' PROVJERA DA LI JE POTPIS NALOGODAVCA ( pInData.signPos = 4 )
        If pInData.signPos = 4 AndAlso pInData.pnDoc = 2 Then

            ' AKO JESTE, PROVJERA DA LI IMA POTPIS IZVJEŠTAJA ( pInData.signPos = 7 )
            Dim _signPos7 As Integer = 7
            ' Provjera da li je na toj poziciji već upisan potpis: 0-No, 1-Yes
            Dim _isSignedByPos7 = _ApiPNalog.isSignedByPos(pInData.pnID, pInData.pnDoc, _signPos7)

            ' Ako je potpis nalogodavca, bez potpisa izvještaja:
            ' INSERT slike potpisa nalogodavca na mjesto pInData.signPos = 7 
            If _isSignedByPos7 = 0 Then

                Dim _apiCertificateSign As New ngApiCertificateSign()
                _apiCertificateSign.setSignPosition(pInData.pnDoc, _signPos7)

                ' Ako su pozicije potpisa definisane markerom preuzmi 
                If _apiCertificateSign.MarkerPos <> "" Then
                    _apiCertificateSign.getMarkerPosition(_InMemStream)
                End If

                'Dim _imgMemStream As MemoryStream
                '_imgMemStream = _apiCertificateSign.getImage(_ECert.id)
                '_apiCertificateSign.insImg2Doc(_InMemStream, _imgMemStream, _outMemStream)

                Dim _isCorrupted7 = _apiCertificateSign.Sign(_InMemStream, _ECert, _fileName, _outMemStream)
                _InMemStream = _outMemStream
                _outMemStream = New MemoryStream()

                If _isCorrupted7 Then

                    _log = "putnalsignbycert - pdf signed file : HTTP status 409"
                    _comment = "FAILED: pdf SignedByPos7 corrupted: " + _lastSignedFile
                    _ApiSession.insApiLog(_log, _comment)

                    Return New Results.StatusCodeResult(HttpStatusCode.Conflict, Request)
                End If

            End If

        End If
        '      
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''





        '
        'Dim _isCorrupted3b = _ApiCertSign.analyzePdfDocument(_InMemStream).IsCorrupted

        Dim _isCorrupted2 = _ApiCertSign.Sign(_InMemStream, _ECert, _fileName, _outMemStream)
        _apiDbFiles.setRawDataFromStream(_outMemStream)

        If _isCorrupted2 Then

            _log = "putnalsignbycert - pdf signed file : HTTP status 409"
            _comment = "FAILED: pdf corrupted: " + _lastSignedFile
            _ApiSession.insApiLog(_log, _comment)

            Return New Results.StatusCodeResult(HttpStatusCode.Conflict, Request)
        End If

        _log = "putnalsignbycert - signed and saved"
        _comment = "SUCCESS: file saved: " + _fileName
        _ApiSession.insApiLog(_log, _comment)

        'TODO: na osnovu vrijednosti _filepath, birati db ili fs
        '''' Spasi pdf u FILE SISTEM / i meta podatke u DB !!!                                       ' -------> filepath bez  HttpContext  
        _apiDbFiles.writePdf2File(pInData.pnID, pInData.pnDoc, pInData.signPos, _ECert.id, _fileName, _filePath)

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '   SLANJE EMAIL PORUKE O PROMJENI STATUSA

        Dim _apiEmailNotify = New ngApiEmailNotify(_ApiSession)
        _apiEmailNotify.prepForEmailBySigner(pInData.pnID, pInData.pnDoc)

        Dim _ApiEmails As New NgApiEmailNET(_ApiSession, "info@mperun.net", "Perun - poslovni portal BHRT")
        _ApiEmails.SendMail(_apiEmailNotify, pInData.pnID, _ApiSession.EmployeeID, _apiEmailNotify.clientEmailAttPath)


        '
        '
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

        Return Json(_replyMess)

    End Function

    ' API eD1
    <HttpPost>
    <Authorize>
    Public Function PutNalUnSignByCert(<FromBody> ByVal pInData As ngPnPotpisByCert) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' 

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' LOGGING
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''




        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' PROVJERA PRAVA NA brisanje potpisa
        '
        Dim _ApiPNalog As New NgApiPnPutniNalog()
        Dim _ApiCertSign As New ngApiCertificateSign()
        Dim _ApiCertificate As New ngApiCertificate()

        ' Uzmi ID e-potpisa logovanog usera
        '
        Dim _ESignId As Integer = _ApiCertificate.getId(_ApiSession.EmployeeID)

        ' User uopšte nema potpisa
        If (_ESignId = Nothing) Then
            Return New Results.StatusCodeResult(HttpStatusCode.NotAcceptable, Request)
        End If

        ' Uzmi ID e-potpisa zadnjeg potpisa
        '
        Dim _lastESignId As Integer = _ApiPNalog.getLastSignedESignID(pInData.pnID, pInData.pnDoc)

        ' Dokument uopšte nema potpisa
        If (_lastESignId = Nothing) Then
            Return New Results.StatusCodeResult(HttpStatusCode.NotAcceptable, Request)
        End If

        ' ID zadnjeg potpisa na dokumentu ne odgovara ID e-potpisa usera
        '
        If (_ESignId <> _lastESignId) Then
            Return New Results.StatusCodeResult(HttpStatusCode.NotAcceptable, Request)
        End If

        ' Uzmi poziciju zadnjeg potpisa
        '
        Dim _lastSignedPos As Integer = _ApiPNalog.getLastSignedPos(pInData.pnID, pInData.pnDoc)
        If (_lastSignedPos = Nothing) Then
            Return New Results.StatusCodeResult(HttpStatusCode.NotAcceptable, Request)
        Else
            pInData.signPos = _lastSignedPos
        End If

        ' Uzmi naziv zadnjeg fajla
        '
        Dim _lastSignedFile As String = _ApiPNalog.getLastSignedFile(pInData.pnID, pInData.pnDoc)
        If (_lastSignedFile.Length = 0) Then
            Return New Results.StatusCodeResult(HttpStatusCode.NotAcceptable, Request)
        End If

        Dim _ApiPutniNalog As New NgApiPnPutniNalog
        Dim _replyMess As Boolean = False

        ' Obriši iz tabela: 
        '                   putninalog_potpisi
        '                   putninalog_doc_documents    (kao i file)
        '
        ' ne dira se :      putninalog_hash
        '
        pInData.employeeID = _ApiSession.EmployeeID
        _replyMess = _ApiPutniNalog.delPutNalESignature(pInData, _ESignId)
        If (_replyMess = True) Then
            _replyMess = _ApiPutniNalog.delPutNalPdfByCert(pInData, _ESignId)
            'File.Delete(_lastSignedFile)
        End If

        _lastESignId = _ApiPNalog.getLastSignedESignID(pInData.pnID, pInData.pnDoc)
        '
        ' Obrisani su svi potpisi, obriši i hash
        '
        If (_lastESignId = Nothing) AndAlso (_replyMess = True) Then
            _replyMess = _ApiPutniNalog.delPutNalHash(pInData)
            '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            ' IIS Worker lockuje file te pravi exception
            Try
                File.Delete(_lastSignedFile)
            Catch ex As Exception

            End Try
        End If

        _log = "putnalUNsignbycert - unsigned and deleted"
        _comment = "SUCCESS: file deleted: " + _lastSignedFile
        _ApiSession.insApiLog(_log, _comment)
        '
        '
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

        Return Json(_replyMess)

    End Function

    ' API esC2 - potpisuje pdf certifikatom pfx
    <HttpGet>
    <Authorize>
    Public Function PutNalisSignedByPos(<FromUri> ByVal pnId As Integer, ByVal pnDoc As Integer, ByVal signPos As Integer) As IHttpActionResult
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parList As String = ""

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "list"
                    _parList = keypair.Value
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)

        ' LOGGING
        '
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '
        Dim _ApiPNalog As New NgApiPnPutniNalog()


        ' Provjera da li je na toj poziciji već upisan potpis: 0-No, 1-Yes
        Dim _isSignedByPos = _ApiPNalog.isSignedByPos(pnId, pnDoc, signPos)

        If _isSignedByPos = -1 Then Return New Results.StatusCodeResult(HttpStatusCode.NotAcceptable, Request)

        Return Json(_isSignedByPos)

    End Function

    ' API TEST
    <HttpGet>
    <Authorize>
    Public Function ECertTest() As IHttpActionResult
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()
        Dim _parEmpID As Integer = -1

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "pEmployeeID"
                    If Not (Integer.TryParse(keypair.Value, _parEmpID) = True) Then
                        _parEmpID = -2
                    End If
            End Select
        Next

        '-------------------------------------------------------------------------------------------------------
        ' SESSION LOG 
        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)
        '-------------------------------------------------------------------------------------------------------

        Dim _ApiCertificate As New ngApiCertificate()
        Dim _ApiDbFiles As New ngApiDbFiles
        Dim _ECertificate As New NgECertificate
        _ECertificate.employeeID = _parEmpID

        ' Uzmi id certifikata
        Dim _isReady = _ApiCertificate.isDbReady(_ECertificate)

        ' Pročitaj img iz baze, i pretvori u MemoryStream
        Dim cRawData() As Byte = _ApiDbFiles.readPotpisImgFromDb(_ECertificate.id)
        Dim dataStream As MemoryStream = New MemoryStream(cRawData)

        'Dim _apiReport As New ngApiReport

        ' Daj naziv fajlu
        Dim _reportFile = String.Format("img_{0}.png", _parEmpID)
        'Dim _reqFile As String = HttpContext.Current.Server.MapPath("~/Data/" + _reportFile)


        'Dim _dataBytes As Byte() = File.ReadAllBytes(_reqFile)
        'Dim dataStream = New MemoryStream(_dataBytes)

        Return New ngAPIFileResult(dataStream, Request, _reportFile)

        'Dim unserializedContent = _ApiDbFiles.readPotpisPfxFromDb(50, _PfxFileOut)
        'unserializedContent += _ApiDbFiles.readPotpisImgFromDb(50, _ImgFileOut)

        'Return Json(unserializedContent)

    End Function

    ' API TEST - FILE UPLOAD
    <HttpPost>
    <Authorize>
    Public Function ECertTest2() As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        'If Not Request.Content.IsMimeMultipartContent() Then Throw New HttpResponseException(HttpStatusCode.UnsupportedMediaType)

        '' 
        ''
        ''
        Dim _ApiDbFiles As New ngApiDbFiles

        Dim _ApiReport As New ngApiReport
        Dim destDir As String = ngApiPath.getCurrentPath()

        Dim httpRequest = HttpContext.Current.Request

        For Each file As String In httpRequest.Files
            Dim postedFile = httpRequest.Files(file)


            Dim _fs As Stream = postedFile.InputStream
            Dim _br As BinaryReader = New BinaryReader(_fs)
            Dim _bytes As Byte() = _br.ReadBytes(CType(_fs.Length, Int32))
            'Dim filePath As String = "~/Files/" & Path.GetFileName(postedFile.FileName)
            'file.WriteAllBytes(Server.MapPath(filePath), bytes)

            '_ApiDbFiles.writTestFILE2Db(67, _bytes, _fs.Length)

            'Dim filePath = destDir & postedFile.FileName
            'postedFile.SaveAs(filePath)
        Next

        Dim _PfxFileOut As String = destDir + "test_66_pdf.pdf"
        'Dim unserializedContent = _ApiDbFiles.readFilePdfFromDb(66, _PfxFileOut)

        Return Ok()

    End Function





    ' API TEST - ...
    <HttpGet>
    <Authorize>
    Public Function testapi(ByVal pnId As Integer, ByVal pnStatus As String, ByVal pnVrsta As Integer) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)

        'Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _methodName
        Dim _comment As String = "api ..."
        _ApiSession.insApiLog(_log, _comment)

        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '
        '   Slanje emaila nakon promjene statusa za nalog pnId - pnStatus.
        '   
        '   Preuzima role kojima će se slati email iz tabele putninalog_workflow, parametar pnStatus se upoređuje sa kolonom status_to
        '   Prema org šifri uposlenika pregledaju se izlistane role koje imaju org role koje obuhvataju org šifru uposlenika
        '   Tako se dobija lista email adresa na koje treba slati email poruku
        '   
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '
        '   API code
        '

        Dim _apiEmailNotify = New ngApiEmailNotify(_ApiSession)
        Dim _emailNotifyOk = _apiEmailNotify.prepForEmailByEmp(pnId, pnStatus, pnVrsta, _ApiSession.EmployeeID)

        If _emailNotifyOk = True Then
            Dim _ApiEmails As New NgApiEmailNET(_ApiSession, "info@mperun.net", "Perun - poslovni portal BHRT")
            _ApiEmails.SendMail(_apiEmailNotify, pnId, _ApiSession.EmployeeID)
        End If

        Dim unserializedContent = _apiEmailNotify

        Return Json(unserializedContent)
    End Function

    <HttpGet>
    <Authorize>
    Public Function testapi2(ByVal pnId As Integer, ByVal pnDoc As Integer) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)

        'Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _methodName
        Dim _comment As String = "api ..."
        _ApiSession.insApiLog(_log, _comment)

        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '
        '   Slanje emaila nakon potpisivanja za nalog pnId - pnDoc
        '   Preuzima zadnji potpisani nalog sa servera, i šalje ga kao att potpisniku 

        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '
        '   API code
        '

        Dim _apiEmailNotify = New ngApiEmailNotify(_ApiSession)
        _apiEmailNotify.prepForEmailBySigner(pnId, pnDoc)

        Dim _ApiEmails As New NgApiEmailNET(_ApiSession, "info@mperun.net", "Perun - poslovni portal BHRT")
        _ApiEmails.SendMail(_apiEmailNotify, pnId, _ApiSession.EmployeeID, _apiEmailNotify.clientEmailAttPath)



        Dim unserializedContent = _apiEmailNotify

        Return Json(unserializedContent)
    End Function

    <HttpGet>
    <Authorize>
    Public Function testapi3(<FromBody> ByVal pInData As ngPnPotpisByCert) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role
        Dim _comment As String = "Api: " + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)




        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' PROVJERA PRAVA NA POTPIS
        '
        Dim _ApiPNalog As New NgApiPnPutniNalog()
        Dim _ApiCertSign As New ngApiCertificateSign()
        Dim _ApiCertificate As New ngApiCertificate()

        ' PRAVO NA POTPIS OK - INIT

        Dim _ECert As New NgECertificate
        _ECert.employeeID = _ApiSession.EmployeeID              ' Potpis se može davati samo iz svoje sesije !!!
        _ECert.password = pInData.password

        Dim _isReady = _ApiCertificate.isDbReady(_ECert)        ' provjera da li ima pfx certifikat ?? SAMO PROVJERAVA DA LI IMA SLIKU POTPISA
        If (_isReady = False) Then
            Return New Results.StatusCodeResult(HttpStatusCode.NotAcceptable, Request)
        End If

        ' Provjera da li je dokument već potpisan od signer-a: 0-No, 1-Yes
        Dim _isSignedBySigner As Integer = _ApiPNalog.isSignedBySigner(pInData.pnID, pInData.pnDoc, _ECert.id)
        If (_isSignedBySigner > 0) Then
            Return New Results.StatusCodeResult(HttpStatusCode.NotAcceptable, Request)
        End If

        ' *** uzimanje POZICIJE POTPISA iz json i max broja pozicija potpisa
        '
        _ApiCertSign.setSignPosition(pInData.pnDoc, pInData.signPos)

        ' Provjera ukupnog mogućeg broja potpisa na dokumentu
        If (pInData.signPos > _ApiCertSign.MaxPos) Then
            Return New Results.StatusCodeResult(HttpStatusCode.NotAcceptable, Request)
        End If

        ' Provjera da li je na toj poziciji već upisan potpis: 0-No, 1-Yes
        Dim _isSignedByPos As Integer = _ApiPNalog.isSignedByPos(pInData.pnID, pInData.pnDoc, pInData.signPos)

        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' IZUZETAK: ako se traži pInData.signPos = 7, provjeriti da li je pozicija 4 aktivna. Ako jeste, vratiti 1

        If pInData.signPos = 7 AndAlso _isSignedByPos = 0 Then
            Dim _signPos4 As Integer = 4
            _isSignedByPos = _ApiPNalog.isSignedByPos(pInData.pnID, pInData.pnDoc, _signPos4)
        End If
        '
        '
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''


        If _isSignedByPos > 0 Then
            Return New Results.StatusCodeResult(HttpStatusCode.NotAcceptable, Request)
        End If


        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' E-POTPIS PODATAKA: upisuje u tabelu putninalog_potpisi
        '
        Dim pESignature As New NgESignature
        pESignature.pPutNalID = pInData.pnID
        pESignature.pEmployeeID = _ApiSession.EmployeeID    ' Postavi pEmpID koji pripada userusesije 
        pESignature.pPutNalESignature = "01234567890"       ' Ne postoji ESignature kao kod 1 vrste potpisa, ovo je Dummy

        ' Uzmi putni nalog
        Dim _pn As ngPnPutniNalog = _ApiPNalog.getPnPotpis_1_ById(pInData.pnID)

        'TODO: _pnJSON treba da bude zavisno o kojoj se vrsti dokumenta radi: 1 ili 2

        Dim _pnJSON As String = JsonConvert.SerializeObject(_pn)
        '
        ' Provjeri i upiši u DB potpis
        Dim _replyMess = _ApiPNalog.signPutNalByCert2(pESignature, _pnJSON, _ECert, pInData, True)

        '
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' UZIMANJE PDF NALOGA
        '
        ' Uzmi poziciju i naziv zadnjeg potpisanog pdf dokumenta
        '
        Dim _lastSignedFile As String = _ApiPNalog.getLastSignedFile(pInData.pnID, pInData.pnDoc)       ' puni path (sa  HttpContext) i naziv fajla

        Dim _apiDbFiles As New ngApiDbFiles

        Dim _outMemStream As New MemoryStream()
        Dim _InMemStream As MemoryStream = New MemoryStream()

        Dim _fileName As String = _apiDbFiles.getRandomFileName(pInData.pnID, pInData.pnDoc)            ' formira naziv fajla
        Dim _filePath As String = "~/Data/PutNal/"                                                             ' path fajla
        Dim _fileDestDir As String = HttpContext.Current.Server.MapPath(_filePath)

        'Dim _rptFile As String = If(pInData.pnDoc = 1, "putnal_69_input.pdf", "putnal_69_input21.pdf")                                                               ' DEVELOPING - COMMENT
        Dim _rptCode As String = If(pInData.pnDoc = 1, "PTNL1", "PTNL2")

        Select Case _lastSignedFile
            Case ""  ' uzmi nepotpisani file preko CrystalReports
                Dim _apiReport As New ngApiReport
                '
                ' KREIRANJE REPORTA KAO FILE STREAM UMJESTO FILE !!! NA CLOUD SERVERU IZBACUJE GREŠKU

                '''' !!!
                Dim destDir As String = ngApiPath.getCurrentPath()

                Dim _rptFile As String = _apiReport.createReport(pInData.pnID, _rptCode, _rptCode)                                                                     ' PRODUCTION - UNCOMMENT
                Dim _rptFileStream = New FileStream(destDir + _rptFile, IO.FileMode.Open, IO.FileAccess.Read)                                                          ' PRODUCTION - UNCOMMENT

                'Dim _rptFile As String = "putnal_142_ubple5nut4r.pdf"                                                                                                   ' DEVELOPING - COMMENT
                'Dim _rptFileStream = New FileStream(HttpContext.Current.Server.MapPath("~/Data/PutNal/") + _rptFile, IO.FileMode.Open, IO.FileAccess.Read)              ' DEVELOPING - COMMENT
                _rptFileStream.CopyTo(_InMemStream)


                ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
                '   INSERT SLIKE POTPISA NA POZICIJE NALOGODAVCA - OBRAČUN NALOGA
                '   pInData.pnDoc = 2
                '                   (1) uzmi pozicije potpisa i NgECertificate.id _ECert.id radi uzimanja slike 
                '                       ngApiCertificateSign(.getImage(_ECert.id)
                '                   (2) upiši na ekvivalentne pozicije potpisa za pnDoc = 2: id +100 (101, 102, ...)


                If pInData.pnDoc = 2 Then
                    Dim __ApiCertSign As New ngApiCertificateSign()

                    Dim _signedDoc As Integer = 1                                                                               ' uzmi potpise iz pn za putovanje 
                    Dim _signPosCertId As Dictionary(Of Integer, Integer) = _ApiPNalog.getSignedPos(pInData.pnID, _signedDoc)   ' uzmi pozicije potpisa iz tog naloga

                    Dim _offset As Integer = 100
                    __ApiCertSign.insSignImges2Doc(_signPosCertId, pInData.pnDoc, _InMemStream, _outMemStream, _offset)
                End If

                '
                ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

            Case Else  ' uzmi potpisani file iz putninalog_doc_documents 

                '''' Uzmi pdf iz FILE SISTEMA !!!
                Dim _lastSignedStream = New FileStream(_lastSignedFile, IO.FileMode.Open, IO.FileAccess.Read)
                _lastSignedStream.CopyTo(_InMemStream)

                Dim _isCorrupted1 = _ApiCertSign.analyzePdfDocument(_InMemStream).IsCorrupted
                If _isCorrupted1 Then

                    _log = "putnalsignbycert - last signed file : HTTP status 409"
                    _comment = "FAILED: pdf corrupted: " + _lastSignedFile
                    _ApiSession.insApiLog(_log, _comment)

                    Return New Results.StatusCodeResult(HttpStatusCode.Conflict, Request)
                End If

        End Select
        '
        '
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' E-POTPIS PDF NALOGA I SPAŠAVANJE
        '
        ' Ako su pozicije potpisa definisane markerom preuzmi 
        If _ApiCertSign.MarkerPos <> "" Then
            _ApiCertSign.getMarkerPosition(_InMemStream)
        End If

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '
        '   INSERT SLIKE POTPISA NA IZVJEŠTAJ - pozicija 7 (ako ova pozicija nije potpisana)


        ' PROVJERA DA LI JE POTPIS NALOGODAVCA ( pInData.signPos = 4 )
        If pInData.signPos = 4 Then

            ' AKO JESTE, PROVJERA DA LI IMA POTPIS IZVJEŠTAJA ( pInData.signPos = 7 )
            Dim _signPos7 As Integer = 7
            ' Provjera da li je na toj poziciji već upisan potpis: 0-No, 1-Yes
            Dim _isSignedByPos7 = _ApiPNalog.isSignedByPos(pInData.pnID, pInData.pnDoc, _signPos7)

            ' Ako je potpis nalogodavca, bez potpisa izvještaja:
            ' INSERT slike potpisa nalogodavca na mjesto pInData.signPos = 7 
            If _isSignedByPos7 = 0 Then

                Dim _apiCertificateSign As New ngApiCertificateSign()
                _apiCertificateSign.setSignPosition(pInData.pnDoc, _signPos7)

                ' Ako su pozicije potpisa definisane markerom preuzmi 
                If _apiCertificateSign.MarkerPos <> "" Then
                    _apiCertificateSign.getMarkerPosition(_InMemStream)
                End If

                Dim _imgMemStream As MemoryStream
                _imgMemStream = _apiCertificateSign.getImage(_ECert.id)
                _apiCertificateSign.insImg2Doc(_InMemStream, _imgMemStream, _outMemStream)

                _InMemStream = _outMemStream
                _outMemStream = New MemoryStream()

            End If

        End If
        '      
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''





        '
        Dim _isCorrupted3b = _ApiCertSign.analyzePdfDocument(_InMemStream).IsCorrupted

        Dim _isCorrupted2 = _ApiCertSign.Sign(_InMemStream, _ECert, _fileName, _outMemStream)
        _apiDbFiles.setRawDataFromStream(_outMemStream)

        If _isCorrupted2 Then

            _log = "putnalsignbycert - pdf signed file : HTTP status 409"
            _comment = "FAILED: pdf corrupted: " + _lastSignedFile
            _ApiSession.insApiLog(_log, _comment)

            Return New Results.StatusCodeResult(HttpStatusCode.Conflict, Request)
        End If

        _log = "putnalsignbycert - signed and saved"
        _comment = "SUCCESS: file saved: " + _fileName
        _ApiSession.insApiLog(_log, _comment)

        'TODO: na osnovu vrijednosti _filepath, birati db ili fs
        '''' Spasi pdf u FILE SISTEM / i meta podatke u DB !!!                                       ' -------> filepath bez  HttpContext  
        _apiDbFiles.writePdf2File(pInData.pnID, pInData.pnDoc, pInData.signPos, _ECert.id, _fileName, _filePath)

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '   SLANJE EMAIL PORUKE O PROMJENI STATUSA

        Dim _apiEmailNotify = New ngApiEmailNotify(_ApiSession)
        _apiEmailNotify.prepForEmailBySigner(pInData.pnID, pInData.pnDoc)

        Dim _ApiEmails As New NgApiEmailNET(_ApiSession, "info@mperun.net", "Perun - poslovni portal BHRT")
        _ApiEmails.SendMail(_apiEmailNotify, pInData.pnID, _ApiSession.EmployeeID, _apiEmailNotify.clientEmailAttPath)


        '
        '
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

        Return Json(_replyMess)
    End Function



    Private Function GetDocumentPath(ByVal document As String) As String
        Dim documentPath As String = String.Empty

        If Not System.IO.File.Exists(document) Then
            Dim path = HttpContext.Current.Request.PhysicalApplicationPath
            If System.IO.File.Exists(path & "Data\" & document) Then documentPath = path & "Data\" & document
        Else
            documentPath = document
        End If

        Return documentPath
    End Function


    '-------------------------------------------
    ' Login, Role, Password
    '-------------------------------------------


    ' API wLI1     - Login from web browser
    '<EnableCors("http://localhost:56300", "*", "*")>
    <HttpPost>
    Public Function Login(
        <FromBody> ByVal loginVM As NgLogin) As IHttpActionResult

        'ApiGlobal.domainName = Request.RequestUri().Host
        'ApiGlobal.domainConnectionString = ConfigurationManager.ConnectionStrings(ApiGlobal.domainName).ConnectionString
        ApiGlobal.SetDomain(Request.RequestUri().Host)


        Dim loginResponse = New NgLoginResponse()

        Dim loginrequest = New NgLogin With {
                .Email = loginVM.Email.ToLower(),
                .Password = loginVM.Password,
                .Role = loginVM.Role
            }
        Dim _ApiSession As New NgApiSession

        _ApiSession.Email = loginrequest.Email
        _ApiSession.Role = loginrequest.Role

        Dim isUsernamePasswordValid = _ApiSession.isUsernamePasswordValid(loginrequest.Password)
        'Dim isUsernamePasswordValid = loginrequest.Password = "admin"

        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Login prije autentifikacije..."
        _ApiSession.insApiLog(_log, _comment)


        If isUsernamePasswordValid Then

            If _ApiSession.UserRoles.Contains(loginrequest.Role) Then
                _ApiSession.Role = loginrequest.Role
            End If

            Dim token = _ApiSession.CreateToken()

            Dim _JwtOK As New NgJWTOk
            _JwtOK.success = True
            _JwtOK.token = token
            Dim jsonLwtOk As String = JsonConvert.SerializeObject(_JwtOK, Formatting.None)

            'jsonLwtOk = "{ 'success': true,  'token': 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJkYXRhIjoiYWRtaW4iLCJleHBpcmVzSW4iOiIxaCIsImlhdCI6MTU0NDMxMDE5OX0.A4xmHtGl28lUeaSqLJRqwkcn_w7MQYyFEuHNq2trI8I' }"
            _comment = "Login nakon uspješne autentifikacije..."
            _ApiSession.insApiLog(_log, _comment)

            ApiGlobal.ClearDomain()
            Return Json(_JwtOK)
        End If

        loginResponse.responseMsg.StatusCode = HttpStatusCode.Unauthorized
        Dim response As IHttpActionResult = ResponseMessage(loginResponse.responseMsg)

        ApiGlobal.ClearDomain()
        Return response
    End Function

    '
    ' API wLI2    - RoleChange
    <HttpPost>
    <Authorize>
    Public Function AuthRole(<FromBody> ByVal loginVM As NgLogin) As IHttpActionResult
        Dim loginResponse = New NgLoginResponse()

        Dim loginrequest = New NgLogin With {
                .Role = loginVM.Role
            }

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Promjena role u " + loginrequest.Role + "..."
        _ApiSession.insApiLog(_log, _comment)

        _ApiSession.getEmployeeRoles()

        If _ApiSession.UserRoles.Contains(loginrequest.Role) Then
            _ApiSession.Role = loginrequest.Role

            _comment = "Uspješna promjena role u " + loginrequest.Role + "..."
            _ApiSession.insApiLog(_log, _comment)
        Else

            Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
        End If

        Dim token = _ApiSession.CreateToken()

        Dim _JwtOK As New NgJWTOk
        _JwtOK.success = True
        _JwtOK.token = token
        Dim jsonLwtOk As String = JsonConvert.SerializeObject(_JwtOK, Formatting.None)

        Return Json(_JwtOK)


    End Function


    ' API aUL0     - Podaci o trenutno prijavljenom uposleniku
    <HttpGet>
    <Authorize>
    Public Function GetEmployeeData() As IHttpActionResult


        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _ngApiOrganizacija As New NgApiOrganizacija

        Dim unserializedContent As Object

        If _ApiSession.IsApiRoleAllowed(_className, _methodName) Then

            unserializedContent = _ApiSession.getEmployeeData(_ApiSession.EmployeeID)
            Return Json(unserializedContent)

        Else
            Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
        End If

        Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
    End Function


    ' API aP1     - Change the password
    '<EnableCors("http://localhost:56300", "*", "*")>
    <HttpPost>
    <Authorize>
    Public Function ChangePassword(
        <FromBody> ByVal valueLogin As NgLogin) As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parSendEmail As Boolean = False


        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "pSendEmail"
                    _parSendEmail = IIf(keypair.Value = "true", True, False)
            End Select
        Next


        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + ": " + _className + "." + _methodName
        Dim _comment As String = "Promjena lozinke "
        _ApiSession.insApiLog(_log, _comment)

        Dim _retValue As Boolean = False

        If _ApiSession.IsApiRoleAllowed(_className, _methodName) Then

            Dim _MinRequiredPasswordLength As Integer = _ApiSession.CheckMinRequiredPasswordLength

            If (valueLogin.Password.Length >= _MinRequiredPasswordLength) Then

                _retValue = _ApiSession.ChangePassword(valueLogin.Email, valueLogin.Role, valueLogin.Password)
                _comment = IIf(_retValue = True, "Uspješna promjena lozinke!", "Greška tokom promjene lozinke.")

            Else

                _comment = "Duzina passworda mora biti jednaka ili veca od " + _MinRequiredPasswordLength.ToString() + " karaktera"
            End If


            _ApiSession.insApiLog(_log, _comment)

            If _parSendEmail AndAlso _retValue = True Then

                Dim _parEmployeeId As Integer = _ApiSession.getEmployeeId(valueLogin.Email)
                Dim _ApiEmployee As New NgApiEmployee()
                Dim _employee As NgEmployee = _ApiEmployee.listItem(_parEmployeeId)

                'Dim _ApiEmails As New NgApiEmail()
                Dim _ApiEmails As New NgApiEmailNET(_ApiSession)
                _ApiEmails.setSmtpFrom("info@mperun.net", "Perun - poslovni portal BHRT")


                Dim _keyValues As New Hashtable()
                Select Case _ApiSession.Role
                    Case "uposlenik"
                        _keyValues.Add("%__UserName__%", _employee.emp_username)
                        _keyValues.Add("%__UserPass__%", "bhrt" + _parEmployeeId.ToString)

                        _ApiEmails.sendByTemplate(_parEmployeeId, _keyValues, "email-changepass")


                    Case "administrator"
                        _keyValues.Add("%__UserName__%", _employee.emp_username)
                        _keyValues.Add("%__UserPass__%", "bhrt" + _parEmployeeId.ToString)

                        _ApiEmails.sendByTemplate(_parEmployeeId, _keyValues, "email-resetpass")


                End Select


            End If

        Else

            Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
        End If


        Return Json(_retValue)

    End Function

    ' API aP2     - Reset the password
    '<EnableCors("http://localhost:56300", "*", "*")>
    <HttpPost>
    <Authorize>
    Public Function ResetPassword(
        <FromBody> ByVal valueLogin As NgLogin) As IHttpActionResult


        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + ": " + _className + "." + _methodName
        Dim _comment As String = "Promjena lozinke - reset "
        _ApiSession.insApiLog(_log, _comment)

        Dim _retValue As New NgJWTOk
        _retValue.success = False
        _retValue.token = ""

        If _ApiSession.IsApiRoleAllowed(_className, _methodName) Then

            _retValue.token = _ApiSession.ResetPassword(valueLogin.Email, "")
            _retValue.success = True

            _comment = "Uspješna promjena lozinke - reset!"
            _ApiSession.insApiLog(_log, _comment)
        Else

            Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
        End If


        Dim _retValueJson As String = JsonConvert.SerializeObject(_retValue, Formatting.None)

        Return Json(_retValue)

    End Function

    ' API aUC     - Create user
    '<EnableCors("http://localhost:56300", "*", "*")>
    <HttpGet>
    <Authorize>
    Public Function CreateUser() As IHttpActionResult
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parEmail As String = ""
        Dim _parEmployeeId As Integer = -1
        Dim _parSendEmail As Boolean = False


        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "pEmail"
                    _parEmail = keypair.Value
                Case "pEmployeeId"
                    If Not (Integer.TryParse(keypair.Value, _parEmployeeId) = True) Then
                        _parEmployeeId = -1
                    End If
                Case "pSendEmail"
                    _parSendEmail = IIf(keypair.Value = "true", True, False)
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + ": " + _className + "." + _methodName
        Dim _comment As String = "Kreiranje usera " + _parEmail + " - " + _parEmployeeId.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _retValue As Boolean = False

        If _ApiSession.IsApiRoleAllowed(_className, _methodName) Then

            _retValue = _ApiSession.createUser(_parEmployeeId, _parEmail)

            _comment = "Uspješno kreiran user: " + _parEmail + " - " + _parEmployeeId.ToString
            _ApiSession.insApiLog(_log, _comment)

            'If _parSendEmail AndAlso _retValue = True Then
            '    Dim _ApiEmails As New NgApiEmail()

            '    _ApiEmails.setSmtpFrom()

            'Dim _ApiEmployee As New NgApiEmployee()
            'Dim _employee As NgEmployee = _ApiEmployee.listItem(_parEmployeeId)

            '    _ApiEmails.set1MailTo(_parEmployeeId, "Kreiran pristup portalu")

            '    Dim _body As String
            '    '_body = _ApiEmails.getSmtpTemplateById(2)
            '    _body = _ApiEmails.getSmtpTemplateFromFile("email-creatuser.html")

            '    _body = _body.Replace("%__UserFullName__%", _ApiEmails.recEmailtTo.Item(0).name)
            '    _body = _body.Replace("%__UserName__%", _employee.emp_username)
            '    _body = _body.Replace("%__UserPass__%", "bhrt" + _parEmployeeId.ToString)


            '    Dim unserializedContent = _ApiEmails.SendOneMail(_body)
            'End If

            If _parSendEmail AndAlso _retValue = True Then

                Dim _ApiEmployee As New NgApiEmployee()
                Dim _employee As NgEmployee = _ApiEmployee.listItem(_parEmployeeId)

                'Dim _ApiEmails As New NgApiEmail()
                '_ApiEmails.setSmtpFrom()
                Dim _ApiEmails As New NgApiEmailNET(_ApiSession)
                _ApiEmails.setSmtpFrom("info@mperun.net", "Perun - poslovni portal BHRT")

                Dim _keyValues As New Hashtable()
                _keyValues.Add("%__UserName__%", _employee.emp_username)
                _keyValues.Add("%__UserPass__%", "bhrt" + _parEmployeeId.ToString)

                _ApiEmails.sendByTemplate(_parEmployeeId, _keyValues, "email-creatuser")

            End If
        Else

            Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
        End If


        Return Json(_retValue)

    End Function

    ' API aURA     - Add role to user
    '<EnableCors("http:  //localhost:56300", "*", "*")>
    <HttpPost>
    <Authorize>
    Public Function AddRole(
                           <FromBody> ByVal valueLogin As NgLogin) As IHttpActionResult
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()


        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name
        Dim _log As String = _ApiSession.Email + "-" + ": " + _className + "." + _methodName
        Dim _comment As String = "Dodavanje role " + valueLogin.Role + " - " + _ApiSession.UserId.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _retValue As Boolean = False

        If _ApiSession.IsApiRoleAllowed(_className, _methodName) Then

            _retValue = _ApiSession.AddRole(valueLogin.Role, valueLogin.Email)

            _comment = "Uspješno dodana role: " + valueLogin.Role + " - " + _ApiSession.UserId.ToString
            _ApiSession.insApiLog(_log, _comment)
        Else

            Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
        End If


        Return Json(_retValue)

    End Function

    ' API aURD     - Remove role from user
    '<EnableCors("http://localhost:56300", "*", "*")>
    <HttpPost>
    <Authorize>
    Public Function RemoveRole(
                           <FromBody> ByVal valueLogin As NgLogin) As IHttpActionResult
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()


        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name
        Dim _log As String = _ApiSession.Email + "-" + ": " + _className + "." + _methodName
        Dim _comment As String = "Brisanje role " + valueLogin.Role + " - " + _ApiSession.UserId.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _retValue As Boolean = False

        If _ApiSession.IsApiRoleAllowed(_className, _methodName) Then

            _retValue = _ApiSession.RemoveRole(valueLogin.Role, valueLogin.Email)

            _comment = "Uspješno obrisana rola: " + valueLogin.Role + " - " + _ApiSession.UserId.ToString
            _ApiSession.insApiLog(_log, _comment)

            Return Json(_retValue)
        Else
            'HACK: https://stackoverflow.com/questions/25116032/invalidcastexception-httpresponsemessage-to-ihttpactionresult-on-webapi2-get 
            'Return New HttpResponseMessage(HttpStatusCode.Forbidden)
            Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
        End If


    End Function

    ' API aURU     - set all roles for user
    <HttpPost>
    <Authorize>
    Public Function SetRoles(
                           <FromBody> ByVal pUserRoles As NgUserRoles) As IHttpActionResult


        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name
        Dim _log As String = _ApiSession.Email + "-" + ": " + _className + "." + _methodName
        Dim _comment As String = "Setovanje rola " + " - " + _ApiSession.UserId.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _retValue As Boolean = False

        If _ApiSession.IsApiRoleAllowed(_className, _methodName) Then

            _retValue = _ApiSession.setRoles(pUserRoles, pUserRoles.Email)

            _comment = "Uspješno setovane rola: " + " - " + _ApiSession.UserId.ToString
            _ApiSession.insApiLog(_log, _comment)

            Return Json(_retValue)
        Else
            'HACK: https://stackoverflow.com/questions/25116032/invalidcastexception-httpresponsemessage-to-ihttpactionresult-on-webapi2-get 
            'Return New HttpResponseMessage(HttpStatusCode.Forbidden)
            Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
        End If


    End Function


    ' API aURL1     - List of employees
    <HttpGet>
    <Authorize>
    Public Function GetEmployeeRolesList() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parList As String = ""

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "list"
                    _parList = keypair.Value
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _ngApiOrganizacija As New NgApiOrganizacija

        Dim _tmpTable As String = "tmp_" + _ApiSession.EmployeeID.ToString + "_apissesion"
        Dim unserializedContent As Object

        If _ApiSession.IsApiRoleAllowed(_className, _methodName) Then
            Select Case _parList
                Case "byorg"


                    ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                    '
                    Dim _roleOrgIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email)
                    '
                    ' Uzmi rekurzivno sve org id u hijerarhiji organizacije
                    _ngApiOrganizacija.GetOrgSifra(_roleOrgIds, 5, 1)

                    Dim _orgIds As String = String.Join(",", _ngApiOrganizacija.lstOrgSifras)


                    If _orgIds.Length > 0 Then
                        unserializedContent = _ApiSession.getEmployeeRolesList(_orgIds, "", _tmpTable, _ApiSession.EmployeeID)
                        Return Json(unserializedContent)
                    Else

                        Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
                    End If


                Case "bypg"

                    ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                    '
                    Dim _rolePGIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email, True)
                    '
                    _ngApiOrganizacija.GetPGEmpIds(_rolePGIds)
                    Dim _PGIds As String = String.Join(",", _ngApiOrganizacija.lstEmpIds)

                    If _PGIds.Length > 0 Then
                        unserializedContent = _ApiSession.getEmployeeRolesList("", _PGIds, _tmpTable, _ApiSession.EmployeeID)
                        Return Json(unserializedContent)
                    Else

                        Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
                    End If

                Case "byorgpg"


                    ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                    '
                    Dim _roleOrgIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email)

                    '
                    ' Uzmi rekurzivno sve org id u hijerarhiji organizacije
                    _ngApiOrganizacija.GetOrgSifra(_roleOrgIds, 5, 1)
                    Dim _orgIds As String = String.Join(",", _ngApiOrganizacija.lstOrgSifras)

                    Dim _rolePGIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email, True)
                    _ngApiOrganizacija.GetPGEmpIds(_rolePGIds)
                    Dim _PGIds As String = String.Join(",", _ngApiOrganizacija.lstEmpIds)


                    If _orgIds.Length > 0 Or _PGIds.Length > 0 Then
                        unserializedContent = _ApiSession.getEmployeeRolesList(_orgIds, _PGIds, _tmpTable, _ApiSession.EmployeeID)
                        Return Json(unserializedContent)
                    Else

                        Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
                    End If


            End Select
        Else

            Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
        End If

        Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
    End Function


    ' API aUOS - spasi podatke org role
    '<EnableCors("http://localhost:56300", "*", "*")>
    <HttpPost>
    <Authorize>
    Public Function OrgRoleWrite(
                           <FromBody> ByVal valueLogin As NgLogin) As IHttpActionResult
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()


        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name
        Dim _log As String = _ApiSession.Email + "-" + ": " + _className + "." + _methodName
        Dim _comment As String = "Spašavanje org role " + valueLogin.Role + " - " + _ApiSession.UserId.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _retValue As Boolean = False

        If _ApiSession.IsApiRoleAllowed(_className, _methodName) Then

            _retValue = _ApiSession.setOrgRole(valueLogin.Email, valueLogin.Role, valueLogin.Password)

            _comment = "Uspješno spašena org rola: " + valueLogin.Role + " - " + _ApiSession.UserId.ToString
            _ApiSession.insApiLog(_log, _comment)

            Return Json(_retValue)
        Else

            Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
        End If


    End Function

    ' API aUOL1     - List of employees
    <HttpGet>
    <Authorize>
    Public Function GetEmployeeOrgRolesList() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parList As String = ""

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "list"
                    _parList = keypair.Value
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _ngApiOrganizacija As New NgApiOrganizacija

        Dim _tmpTable As String = "tmp_" + _ApiSession.EmployeeID.ToString + "_apissesion"
        Dim unserializedContent As Object

        If _ApiSession.IsApiRoleAllowed(_className, _methodName) Then
            Select Case _parList
                Case "byorg"


                    ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                    '
                    Dim _roleOrgIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email)
                    '
                    ' Uzmi rekurzivno sve org id u hijerarhiji organizacije
                    _ngApiOrganizacija.GetOrgSifra(_roleOrgIds, 5, 1)

                    Dim _orgIds As String = String.Join(",", _ngApiOrganizacija.lstOrgSifras)


                    If _orgIds.Length > 0 Then
                        unserializedContent = _ApiSession.getEmployeeOrgRolesList(_orgIds, "", _tmpTable, _ApiSession.EmployeeID)
                        Return Json(unserializedContent)
                    Else

                        Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
                    End If


                Case "bypg"

                    ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                    '
                    Dim _rolePGIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email, True)
                    '
                    _ngApiOrganizacija.GetPGEmpIds(_rolePGIds)
                    Dim _PGIds As String = String.Join(",", _ngApiOrganizacija.lstEmpIds)

                    If _PGIds.Length > 0 Then
                        unserializedContent = _ApiSession.getEmployeeOrgRolesList("", _PGIds, _tmpTable, _ApiSession.EmployeeID)
                        Return Json(unserializedContent)
                    Else

                        Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
                    End If

                Case "byorgpg"


                    ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                    '
                    Dim _roleOrgIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email)

                    '
                    ' Uzmi rekurzivno sve org id u hijerarhiji organizacije
                    _ngApiOrganizacija.GetOrgSifra(_roleOrgIds, 5, 1)
                    Dim _orgIds As String = String.Join(",", _ngApiOrganizacija.lstOrgSifras)

                    Dim _rolePGIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email, True)
                    _ngApiOrganizacija.GetPGEmpIds(_rolePGIds)
                    Dim _PGIds As String = String.Join(",", _ngApiOrganizacija.lstEmpIds)

                    '_ngApiOrganizacija.getOrgRole(_ApiSession.Email, _roleOrgIds, _rolePGIds)
                    'Dim _regExpOrgSfr As String = _ngApiOrganizacija.sqlPrepRegExpOrg(_roleOrgIds)

                    If _orgIds.Length > 0 Or _PGIds.Length > 0 Then
                        unserializedContent = _ApiSession.getEmployeeOrgRolesList(_orgIds, _PGIds, _tmpTable, _ApiSession.EmployeeID)
                        Return Json(unserializedContent)
                    Else

                        Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
                    End If


            End Select
        Else

            Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
        End If

        Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
    End Function


    ' API aUD     - Delete user
    '<EnableCors("http://localhost:56300", "*", "*")>
    <HttpGet>
    <Authorize>
    Public Function DeleteUser() As IHttpActionResult
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parEmail As String = ""
        Dim _parMobUser As Boolean = False

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "pEmail"
                    _parEmail = keypair.Value
                Case "pMobUser"
                    _parMobUser = IIf(keypair.Value = "true", True, False)
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + ": " + _className + "." + _methodName
        Dim _comment As String = "Brisanje usera " + _parEmail + " - " + _ApiSession.UserId.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _retValue As Boolean = False

        If _ApiSession.IsApiRoleAllowed(_className, _methodName) Then

            If _parMobUser Then
                _retValue = _ApiSession.deleteMobUser(_parEmail)
            Else
                _retValue = _ApiSession.deleteUser(_parEmail)
            End If

            _comment = "Uspješno obrisan " + IIf(_parMobUser, " mobilni ", "") + "user: " + _parEmail + " - " + _ApiSession.UserId.ToString
            _ApiSession.insApiLog(_log, _comment)

            Return Json(_retValue)
        Else

            Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
        End If

    End Function


    ' API aUW     - WebAccessDelete user
    '<EnableCors("http://localhost:56300", "*", "*")>
    <HttpGet>
    <Authorize>
    Public Function SetWebAccess() As IHttpActionResult
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parEmail As String = ""
        Dim _parWebAccess As Boolean = False ' False - nije dozvoljen


        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "pEmail"
                    _parEmail = keypair.Value
                Case "pWebAccess"
                    _parWebAccess = keypair.Value
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + ": " + _className + "." + _methodName
        Dim _comment As String = "Set web access " + _parEmail + " - " + _parWebAccess.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _retValue As Boolean = False

        If _ApiSession.IsApiRoleAllowed(_className, _methodName) Then

            Select Case _parWebAccess
                Case True
                    _retValue = _ApiSession.unlockUser(_parEmail)

                Case False
                    _retValue = _ApiSession.lockUser(_parEmail)

            End Select


            _comment = "Uspješno postavljen web access user: " + _parEmail + " - " + _parWebAccess.ToString
            _ApiSession.insApiLog(_log, _comment)

            Return Json(_retValue)
        Else

            Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
        End If

    End Function

    ' API aUL1     - List of employees
    <HttpGet>
    <Authorize>
    Public Function GetEmployeeList() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parList As String = ""
        Dim _parVrsta As Integer = -1
        Dim _parOrgSifra As String = ""

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "list"
                    _parList = keypair.Value
                Case "vrsta"
                    If Not (Integer.TryParse(keypair.Value, _parVrsta) = True) Then
                        _parVrsta = -1
                    End If
                Case "sifra"
                    _parOrgSifra = keypair.Value
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _ngApiOrganizacija As New NgApiOrganizacija

        Dim _tmpTable As String = "tmp_" + _ApiSession.EmployeeID.ToString + "_apissesion"
        Dim unserializedContent As Object

        If _ApiSession.IsApiRoleAllowed(_className, _methodName) Then
            Select Case _parList
                Case "byorg"


                    ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                    '
                    Dim _roleOrgIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email)
                    '
                    ' Uzmi rekurzivno sve org id u hijerarhiji organizacije
                    _ngApiOrganizacija.GetOrgSifra(_roleOrgIds, 5, 1)

                    Dim _orgIds As String = String.Join(",", _ngApiOrganizacija.lstOrgSifras)


                    If _orgIds.Length > 0 Then
                        unserializedContent = _ApiSession.getEmployeeList(_orgIds, "", _tmpTable, _ApiSession.EmployeeID, _parVrsta)
                        Return Json(unserializedContent)
                    Else

                        Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
                    End If

                Case "byorgsifra"

                    Dim _orgIds As String = _parOrgSifra

                    If _orgIds.Length > 0 Then
                        unserializedContent = _ApiSession.getEmployeeList(_orgIds, "", _tmpTable, _ApiSession.EmployeeID, _parVrsta)
                        Return Json(unserializedContent)
                    Else

                        Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
                    End If


                Case "bypg"

                    ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                    '
                    Dim _rolePGIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email, True)
                    '
                    _ngApiOrganizacija.GetPGEmpIds(_rolePGIds)
                    Dim _PGIds As String = String.Join(",", _ngApiOrganizacija.lstEmpIds)

                    If _PGIds.Length > 0 Then
                        unserializedContent = _ApiSession.getEmployeeList("", _PGIds, _tmpTable, _ApiSession.EmployeeID, _parVrsta)
                        Return Json(unserializedContent)
                    Else

                        Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
                    End If

                Case "bypgsifra"

                    _ngApiOrganizacija.GetPGEmpIds(_parOrgSifra)
                    Dim _PGIds As String = String.Join(",", _ngApiOrganizacija.lstEmpIds)

                    If _PGIds.Length > 0 Then
                        unserializedContent = _ApiSession.getEmployeeList("", _PGIds, _tmpTable, _ApiSession.EmployeeID, _parVrsta)
                        Return Json(unserializedContent)
                    Else

                        Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
                    End If

                Case "byorgpg"


                    ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                    '
                    Dim _roleOrgIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email)

                    '
                    ' Uzmi rekurzivno sve org id u hijerarhiji organizacije
                    _ngApiOrganizacija.GetOrgSifra(_roleOrgIds, 5, 1)
                    Dim _orgIds As String = String.Join(",", _ngApiOrganizacija.lstOrgSifras)

                    Dim _rolePGIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email, True)
                    _ngApiOrganizacija.GetPGEmpIds(_rolePGIds)
                    Dim _PGIds As String = String.Join(",", _ngApiOrganizacija.lstEmpIds)


                    If _orgIds.Length > 0 Or _PGIds.Length > 0 Then
                        unserializedContent = _ApiSession.getEmployeeList(_orgIds, _PGIds, _tmpTable, _ApiSession.EmployeeID, _parVrsta)
                        Return Json(unserializedContent)
                    Else

                        Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
                    End If


            End Select
        Else

            Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
        End If

        Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
    End Function


    '-------------------------------------------
    ' Login, Registracija - mPerun
    '-------------------------------------------

    ' API mLI0      - Registracija
    '<EnableCors("http://localhost:56300", "*", "*")>
    <HttpPost>
    Public Function mRegister(
        <FromBody> ByVal _login As NgLoginM) As IHttpActionResult
        Dim loginResponse = New NgLoginResponse()

        Dim loginrequest = New NgLoginM With {
                .Email = _login.Email.ToLower(),
                .Password = _login.Password,
                .Role = _login.Role,
                .UUID = _login.UUID
            }

        Dim _ApiSession As New NgApiSession

        _ApiSession.Email = loginrequest.Email
        _ApiSession.Role = "uposlenik"

        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _JwtOK As New NgJWTOk

        Dim isUsernamePasswordValid = _ApiSession.isUsernamePasswordValid(loginrequest.Password)

        If isUsernamePasswordValid Then

            ' Provjera da li postoji UUID ...za ovaj email
            If _ApiSession.isUUIDindbase(loginrequest.UUID) Then

                ' i da li je isti
                If _ApiSession.UUID = loginrequest.UUID Then
                    ' Već registrovan
                    ' Email obavijest da je već registrovan sa ovog uređaja
                    _JwtOK.success = False
                    _JwtOK.token = "already_registered"

                Else
                    ' Pogrešan UUID
                    ' Email obavijest da je pokušana registracija sa drugog uređaja
                    _JwtOK.success = False
                    _JwtOK.token = "attempted_registration"
                End If

            Else
                ' 
                ' Upiši UUID u bazu - registracija mPerun
                ' _ApiSession.insUUID (UUID)
                If _ApiSession.insUUID(loginrequest.UUID) = True Then
                    _JwtOK.success = True
                    _JwtOK.token = "just_registered"
                Else
                    _JwtOK.success = False
                    _JwtOK.token = "failed_registration"
                End If


            End If


            Dim jsonLwtOk As String = JsonConvert.SerializeObject(_JwtOK, Formatting.None)
            Return Json(_JwtOK)

        End If

        loginResponse.responseMsg.StatusCode = HttpStatusCode.Unauthorized
        Dim response As IHttpActionResult = ResponseMessage(loginResponse.responseMsg)
        Return response

    End Function


    ' API wLI1     - Login from mPerun
    '<EnableCors("http://localhost:56300", "*", "*")>
    <HttpPost>
    Public Function mLogin(
        <FromBody> ByVal _login As NgLoginM) As IHttpActionResult
        Dim loginResponse = New NgLoginResponse()

        Dim loginrequest = New NgLoginM With {
                .Email = _login.Email.ToLower(),
                .Password = _login.Password,
                .Role = _login.Role,
                .UUID = _login.UUID
            }

        Dim _ApiSession As New NgApiSession

        _ApiSession.Email = loginrequest.Email
        _ApiSession.Role = "uposlenik"

        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim isUsernamePasswordValid = _ApiSession.isUsernamePasswordValid(loginrequest.Password)
        'Dim isUsernamePasswordValid = loginrequest.Password = "admin"

        If isUsernamePasswordValid Then

            If _ApiSession.UserRoles.Contains(loginrequest.Role) Then
                _ApiSession.Role = loginrequest.Role
            End If

            '
            ' UUID je validan (upisan u bazu i isti kao u login)
            '
            If _ApiSession.isUUIDindbase(loginrequest.UUID) Then
                If _ApiSession.UUID = loginrequest.UUID Then

                    Dim token = _ApiSession.CreateToken()

                    Dim _JwtOK As New NgJWTOk
                    _JwtOK.success = True
                    _JwtOK.token = token
                    Dim jsonLwtOk As String = JsonConvert.SerializeObject(_JwtOK, Formatting.None)

                    Return Json(_JwtOK)

                End If
            End If


        End If

        loginResponse.responseMsg.StatusCode = HttpStatusCode.Unauthorized
        Dim response As IHttpActionResult = ResponseMessage(loginResponse.responseMsg)
        Return response
    End Function


    '
    ' Protokol, Evidencija, ePotpisi i Parametri
    '

    '
    ' PROTOKOL
    '
    ' API P-L1
    <HttpGet>
    <Authorize>
    Public Function ProtokolEvd() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parOrgJed As String = ""
        Dim _parGodina As Integer = -1


        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "pOrgJed"
                    _parOrgJed = keypair.Value
                Case "pGodina"
                    If Not (Integer.TryParse(keypair.Value, _parGodina) = True) Then
                        _parGodina = -1
                    End If
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '


        Dim _ApiProtokol As New ngApiProtokol()

        'Dim _RolesAllowed() As String = {"uposlenik", "sekretarica", "rukovodilac", "producent", "uprava"}


        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        If _ApiSession.IsApiRoleAllowed(_className, _methodName) Then

            Select Case _allUrlKeyValues.Count
                Case 1
                    _ApiProtokol.listItems(_parGodina)
                Case 2
                    _ApiProtokol.listItems(_parOrgJed, _parGodina)
            End Select

        End If


        Dim unserializedContent = _ApiProtokol.List

        Return Json(unserializedContent)

    End Function

    ' API P-L2
    <HttpGet>
    <Authorize>
    Public Function ProtokolEvdFind() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parOrgJed As String = ""
        Dim _parGodina As Integer = -1


        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "pOrgJed"
                    _parOrgJed = keypair.Value
                Case "pGodina"
                    If Not (Integer.TryParse(keypair.Value, _parGodina) = True) Then
                        _parGodina = -1
                    End If
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        '


        Dim _ApiProtokol As New ngApiProtokol()

        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        'If _ApiSession.IsApiRoleAllowed(_className, _methodName) Then

        Select Case _allUrlKeyValues.Count
            Case 2
                _ApiProtokol.listItem(_parOrgJed, _parGodina, True)
        End Select

        'End If


        Dim unserializedContent = _ApiProtokol.List.Item(0)

        Return Json(unserializedContent)

    End Function


    ' API P-C1
    <HttpPost>
    <Authorize>
    Public Function ProtokolEvdIns(<FromBody()> ByVal valueProtokol As ngPnProtokol) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '

        Dim _ngPnProtokol As ngPnProtokol = valueProtokol
        Dim _newProtookolRB As Integer = -1

        Dim _ApiProtokol As New ngApiProtokol()

        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        If _ApiSession.IsApiRoleAllowed(_className, _methodName) Then
            _newProtookolRB = _ApiProtokol.insItem(_ngPnProtokol)

        End If


        Dim unserializedContent = _ApiProtokol.listItem(_ngPnProtokol.orgjed, _ngPnProtokol.godina)

        Return Json(unserializedContent)

    End Function


    ' API P-S1
    <HttpPost>
    <Authorize>
    Public Function ProtokolEvdWrite(<FromBody()> ByVal valueProtokol As ngPnProtokol) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '
        ' Uzmi iz <body> parametar
        '
        Dim _ngPnProtokol As ngPnProtokol = valueProtokol

        Dim _ApiProtokol As New ngApiProtokol()

        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        If _ApiSession.IsApiRoleAllowed(_className, _methodName) Then
            _ApiProtokol.updItem(_ngPnProtokol)
        End If

        Dim unserializedContent = _ApiProtokol.listItem(_ngPnProtokol.redbr, _ngPnProtokol.godina)

        Return Json(unserializedContent)

    End Function

    ' API P-D1
    <HttpPost>
    <Authorize>
    Public Function ProtokolEvdDel(<FromBody()> ByVal valueProtokol As ngPnProtokol) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '
        Dim _ngPnProtokol As ngPnProtokol = valueProtokol

        Dim _ApiProtokol As New ngApiProtokol()


        Dim _retValue As Boolean = False

        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        If _ApiSession.IsApiRoleAllowed(_className, _methodName) Then
            _retValue = _ApiProtokol.delItem(_ngPnProtokol)
        End If



        Return Json(_retValue)

    End Function


    '
    ' E-POTPISI
    '
    ' API EP-L1
    <HttpGet>
    <Authorize>
    Public Function Potpis() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parAktivan As Integer = -1


        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "pAktivan"
                    _parAktivan = keypair.Value
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '


        Dim _ApiPnPotpisi As New ngApiPnPotpisi()

        Dim _RolesAllowed() As String = {"uposlenik", "sekretarica", "rukovodilac", "producent", "uprava"}
        If _RolesAllowed.Contains(_ApiSession.Role) Then

            Select Case _allUrlKeyValues.Count
                Case 1
                    _ApiPnPotpisi.listItems(_parAktivan)
            End Select

        End If


        Dim unserializedContent = _ApiPnPotpisi.List

        Return Json(unserializedContent)

    End Function


    ' API EP-C1
    <HttpPost>
    <Authorize>
    Public Function PotpisIns(<FromBody()> ByVal valuePotpis As ngPnPotpis) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _ngPnPotpis As ngPnPotpis = valuePotpis
        Dim _newPotpisId As Integer = -1

        Dim _ApiPnPotpisi As New ngApiPnPotpisi()

        Dim _RolesAllowed() As String = {"uposlenik", "sekretarica", "rukovodilac", "producent", "uprava"}

        If _RolesAllowed.Contains(_ApiSession.Role) Then
            _newPotpisId = _ApiPnPotpisi.insItem(_ngPnPotpis)

        End If


        Dim unserializedContent = _ApiPnPotpisi.listItem(_ngPnPotpis.employee_id, _newPotpisId)

        Return Json(unserializedContent)

    End Function


    ' API ORGANIZACIJA

    ' API oL1     - List organizational structure
    <HttpGet>
    <Authorize>
    Public Function GetOrganizacijaList() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parList As String = "all"
        Dim _parIdNum As Boolean = False

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "list"
                    _parList = keypair.Value
                Case "numeric"
                    _parIdNum = IIf(keypair.Value = "true", True, False)
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _ngApiOrganizacija As New NgApiOrganizacija

        Dim _tmpTable As String = "tmp_" + _ApiSession.EmployeeID.ToString + "_apissesion"
        Dim unserializedContent As Object

        If _ApiSession.IsApiRoleAllowed(_className, _methodName) Then
            Select Case _parList

                Case "all"
                    unserializedContent = _ngApiOrganizacija.getList("", _parIdNum)
                    Return Json(unserializedContent)
                Case "obracun"
                    unserializedContent = _ngApiOrganizacija.getList(" WHERE Obracun <> 0 AND Atribut_1='D'", _parIdNum)
                    Return Json(unserializedContent)
                Case "employees"
                    unserializedContent = _ngApiOrganizacija.getList(" WHERE Uposlenici <> 0 AND Atribut_1='D'", _parIdNum)
                    Return Json(unserializedContent)
                Case "pg"
                    unserializedContent = _ngApiOrganizacija.getPGList("")
                    Return Json(unserializedContent)
                Case Else

            End Select
        Else

            Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
        End If

        Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
    End Function


    <HttpGet>
    <Authorize>
    Public Function GetEmailTemplatesList() As IHttpActionResult


        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)



        Dim _lstTemplates = ngApiEmailNotify.getTemplates()



        Return Json(_lstTemplates)
    End Function



    '
    '   --- Aktivnosti ---
    '

    ' API A-L1
    <HttpGet>
    <Authorize>
    Public Function Aktivnost() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parId As Integer = -1

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "id"
                    _parId = keypair.Value
            End Select
        Next


        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '
        'Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)

        Dim _ApiAktivnost = New ngApiAktivnost
        Dim unserializedContent As Object

        If _parId > -1 Then
            _ApiAktivnost.listItem(_parId)
            unserializedContent = _ApiAktivnost.List.Item(0)
        Else
            _ApiAktivnost.listItems()
            unserializedContent = _ApiAktivnost.List
        End If

        Return Json(unserializedContent)

    End Function

    ' API A-C1
    <HttpPost>
    <Authorize>
    Public Function AktivnostIns(<FromBody()> ByVal valuePost As ngAktivnost) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        '

        Dim _ngAktivnost As ngAktivnost = valuePost
        Dim _newId As Integer = -1

        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _ApiAktivnost = New ngApiAktivnost

        _newId = _ApiAktivnost.insItem(_ngAktivnost)

        Dim unserializedContent = _ApiAktivnost.listItem(_newId)
        Return Json(unserializedContent)

    End Function


    ' API A-S1
    <HttpPost>
    <Authorize>
    Public Function AktivnostWrite(<FromBody()> ByVal valuePost As ngAktivnost) As IHttpActionResult
        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        '

        Dim _ngAktivnost As ngAktivnost = valuePost
        Dim _newId As Integer = -1

        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _ApiAktivnost = New ngApiAktivnost


        Dim unserializedContent = _ApiAktivnost.updItem(_ngAktivnost)



        Return Json(unserializedContent)

    End Function

    ' API A-D1
    <HttpPost>
    <Authorize>
    Public Function AktivnostDel(<FromBody()> ByVal valuePost As ngAktivnost) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        '

        Dim _ngAktivnost As ngAktivnost = valuePost
        Dim _newId As Integer = -1

        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _ApiAktivnost = New ngApiAktivnost


        Dim unserializedContent = _ApiAktivnost.delItem(_ngAktivnost)

        Return Json(unserializedContent)

    End Function


    ' ***************************************************
    ' ***** POZIVI VEZANI ZA UPOSLENIKE - Emp ***********
    ' ***************************************************
    '
    '   --- Uposlenici / Saradnici / Korisnici ---
    '

    ' API US-L1, US-L2
    <HttpGet>
    <Authorize>
    Public Function Employee() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parStatus As String = "A"
        Dim _parId As Integer = -1

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "status"
                    _parStatus = keypair.Value
                Case "id"
                    _parId = keypair.Value
            End Select
        Next


        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '
        'Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)

        Dim _ApiEmployee = New NgApiEmployee
        Dim unserializedContent As Object

        If _parId > -1 Then
            _ApiEmployee.listItem(_parId, _parStatus)
            unserializedContent = _ApiEmployee.List.Item(0)
        Else
            _ApiEmployee.listItems(_parStatus)
            unserializedContent = _ApiEmployee.List
        End If

        Return Json(unserializedContent)

    End Function



    ' - List of employees
    ' API US-L3
    '   kod je preuzet od admin koda:  API UL1 GetEmployeeList()

    <HttpGet>
    <Authorize>
    Public Function EmployeesList() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parList As String = ""
        Dim _parVrsta As Integer = -1
        Dim _parOrgSifra As String = ""

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "list"
                    _parList = keypair.Value
                Case "vrsta"
                    If Not (Integer.TryParse(keypair.Value, _parVrsta) = True) Then
                        _parVrsta = -1
                    End If
                Case "sifra"
                    _parOrgSifra = keypair.Value
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _ngApiOrganizacija As New NgApiOrganizacija

        Dim _tmpTable As String = "tmp_" + _ApiSession.EmployeeID.ToString + "_apissesion"
        Dim unserializedContent As Object

        Dim _ApiEmployee = New NgApiEmployee
        'If _ApiSession.IsApiRoleAllowed(_className, _methodName) Then
        Select Case _parList
            Case "byorg"


                ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                '
                Dim _roleOrgIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email)
                '
                ' Uzmi rekurzivno sve org id u hijerarhiji organizacije
                _ngApiOrganizacija.GetOrgSifra(_roleOrgIds, 5, 1)

                Dim _orgIds As String = String.Join(",", _ngApiOrganizacija.lstOrgSifras)


                If _orgIds.Length > 0 Then
                    unserializedContent = _ApiEmployee.listItems(_orgIds, "", _tmpTable, _ApiSession.EmployeeID, _parVrsta)
                    Return Json(unserializedContent)
                Else

                    Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
                End If

            Case "byorgsifra"

                Dim _orgIds As String = _parOrgSifra

                If _orgIds.Length > 0 Then
                    unserializedContent = _ApiEmployee.listItems(_orgIds, "", _tmpTable, _ApiSession.EmployeeID, _parVrsta)
                    Return Json(unserializedContent)
                Else

                    Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
                End If


            Case "bypg"

                ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                '
                Dim _rolePGIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email, True)
                '
                _ngApiOrganizacija.GetPGEmpIds(_rolePGIds)
                Dim _PGIds As String = String.Join(",", _ngApiOrganizacija.lstEmpIds)

                If _PGIds.Length > 0 Then
                    unserializedContent = _ApiEmployee.listItems("", _PGIds, _tmpTable, _ApiSession.EmployeeID, _parVrsta)
                    Return Json(unserializedContent)
                Else

                    Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
                End If

            Case "bypgsifra"

                _ngApiOrganizacija.GetPGEmpIds(_parOrgSifra)
                Dim _PGIds As String = String.Join(",", _ngApiOrganizacija.lstEmpIds)

                If _PGIds.Length > 0 Then
                    unserializedContent = _ApiEmployee.listItems("", _PGIds, _tmpTable, _ApiSession.EmployeeID, _parVrsta)
                    Return Json(unserializedContent)
                Else

                    Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
                End If

            Case "byorgpg"


                ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                '
                Dim _roleOrgIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email)

                '
                ' Uzmi rekurzivno sve org id u hijerarhiji organizacije
                _ngApiOrganizacija.GetOrgSifra(_roleOrgIds, 5, 1)
                Dim _orgIds As String = String.Join(",", _ngApiOrganizacija.lstOrgSifras)

                Dim _rolePGIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email, True)
                _ngApiOrganizacija.GetPGEmpIds(_rolePGIds)
                Dim _PGIds As String = String.Join(",", _ngApiOrganizacija.lstEmpIds)


                If _orgIds.Length > 0 Or _PGIds.Length > 0 Then
                    unserializedContent = _ApiEmployee.listItems(_orgIds, _PGIds, _tmpTable, _ApiSession.EmployeeID, _parVrsta)
                    Return Json(unserializedContent)
                Else

                    Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
                End If


        End Select
        'Else

        '    Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
        'End If

        Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
    End Function


    <HttpGet>
    <Authorize>
    Public Function EmployeesListVar(ByVal pVar As String) As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parList As String = ""
        Dim _parVrsta As Integer = -1
        Dim _parOrgSifra As String = ""

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "list"
                    _parList = keypair.Value
                Case "vrsta"
                    If Not (Integer.TryParse(keypair.Value, _parVrsta) = True) Then
                        _parVrsta = -1
                    End If
                Case "sifra"
                    _parOrgSifra = keypair.Value
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _ngApiOrganizacija As New NgApiOrganizacija

        Dim _tmpTable As String = "tmp_" + _ApiSession.EmployeeID.ToString + "_apissesion"
        Dim unserializedContent As Object

        Dim _ApiEmployee = New NgApiEmployee
        'If _ApiSession.IsApiRoleAllowed(_className, _methodName) Then
        Select Case _parList
            Case "byorg"


                ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                '
                Dim _roleOrgIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email)
                '
                ' Uzmi rekurzivno sve org id u hijerarhiji organizacije
                _ngApiOrganizacija.GetOrgSifra(_roleOrgIds, 5, 1)

                Dim _orgIds As String = String.Join(",", _ngApiOrganizacija.lstOrgSifras)


                If _orgIds.Length > 0 Then
                    unserializedContent = _ApiEmployee.listItems(pVar, _orgIds, "", _tmpTable, _ApiSession.EmployeeID, _parVrsta)
                    Return Json(unserializedContent)
                Else

                    Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
                End If

            Case "byorgsifra"

                Dim _orgIds As String = _parOrgSifra

                If _orgIds.Length > 0 Then
                    unserializedContent = _ApiEmployee.listItems(pVar, _orgIds, "", _tmpTable, _ApiSession.EmployeeID, _parVrsta)
                    Return Json(unserializedContent)
                Else

                    Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
                End If


            Case "bypg"

                ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                '
                Dim _rolePGIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email, True)
                '
                _ngApiOrganizacija.GetPGEmpIds(_rolePGIds)
                Dim _PGIds As String = String.Join(",", _ngApiOrganizacija.lstEmpIds)

                If _PGIds.Length > 0 Then
                    unserializedContent = _ApiEmployee.listItems(pVar, "", _PGIds, _tmpTable, _ApiSession.EmployeeID, _parVrsta)
                    Return Json(unserializedContent)
                Else

                    Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
                End If

            Case "bypgsifra"

                _ngApiOrganizacija.GetPGEmpIds(_parOrgSifra)
                Dim _PGIds As String = String.Join(",", _ngApiOrganizacija.lstEmpIds)

                If _PGIds.Length > 0 Then
                    unserializedContent = _ApiEmployee.listItems(pVar, "", _PGIds, _tmpTable, _ApiSession.EmployeeID, _parVrsta)
                    Return Json(unserializedContent)
                Else

                    Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
                End If

            Case "byorgpg"


                ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                '
                Dim _roleOrgIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email)

                '
                ' Uzmi rekurzivno sve org id u hijerarhiji organizacije
                _ngApiOrganizacija.GetOrgSifra(_roleOrgIds, 5, 1)
                Dim _orgIds As String = String.Join(",", _ngApiOrganizacija.lstOrgSifras)

                Dim _rolePGIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email, True)
                _ngApiOrganizacija.GetPGEmpIds(_rolePGIds)
                Dim _PGIds As String = String.Join(",", _ngApiOrganizacija.lstEmpIds)


                If _orgIds.Length > 0 Or _PGIds.Length > 0 Then
                    unserializedContent = _ApiEmployee.listItems(pVar, _orgIds, _PGIds, _tmpTable, _ApiSession.EmployeeID, _parVrsta)
                    Return Json(unserializedContent)
                Else

                    Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
                End If


        End Select
        'Else

        '    Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
        'End If

        Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
    End Function



    ' API US-C1
    <HttpPost>
    <Authorize>
    Public Function EmployeeIns(<FromBody()> ByVal valuePost As NgEmployee) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        '

        Dim _ngEmployee As NgEmployee = valuePost
        Dim _newId As Integer = -1

        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _ApiEmployee = New NgApiEmployee

        _newId = _ApiEmployee.insItem(_ngEmployee)
        _ngEmployee.EmployeeID = _newId
        '
        ' Zapiši ostale podatke u bazu
        _ApiEmployee.updItem(_ngEmployee)

        Dim unserializedContent = _ApiEmployee.listItem(_newId)
        Return Json(unserializedContent)

    End Function


    ' API US-S1
    <HttpPost>
    <Authorize>
    Public Function EmployeeWrite(<FromBody()> ByVal valuePost As NgEmployee) As IHttpActionResult
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parDataSet As String = "OAB"
        Dim _parDataField As String = ""

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "set"
                    _parDataSet = keypair.Value
                Case "field"
                    _parDataField = keypair.Value
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        '

        Dim _ngEmployee As NgEmployee = valuePost
        Dim _newId As Integer = -1

        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _ApiEmployee = New NgApiEmployee
        Dim _result As Boolean = False
        Select Case _parDataSet
            Case "OAB"
                _result = _ApiEmployee.updItem(_ngEmployee)
            Case "A"
                _result = _ApiEmployee.updItemA(_ngEmployee)
            Case "B"
                ' provjera da li je email jedinstven da bi se koristio kao username
                '
                _result = _ApiEmployee.updItemB(_ngEmployee)
                _result = _ApiEmployee.updItemB1(_ngEmployee)
            Case "C"
                _result = _ApiEmployee.updItemByField(_ngEmployee, _parDataField)
        End Select


        Dim unserializedContent = _result



        Return Json(unserializedContent)

    End Function

    ' API US-D1
    <HttpPost>
    <Authorize>
    Public Function EmployeeDel(<FromBody()> ByVal valuePost As NgEmployee) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        '

        Dim _ngEmployee As NgEmployee = valuePost
        Dim _newId As Integer = -1

        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _ApiAktivnost = New NgApiEmployee


        Dim unserializedContent = _ApiAktivnost.delItem(_ngEmployee)

        Return Json(unserializedContent)

    End Function


    '
    '   --- Općine šifarnik ---
    '

    ' API Op-L1
    <HttpGet>
    <Authorize>
    Public Function Opcina() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parId As Integer = -1
        Dim _parSifra As String = ""

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "id"
                    _parId = keypair.Value
                Case "sifra"
                    _parSifra = keypair.Value
            End Select
        Next


        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '
        'Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)

        Dim _ApiOpcina = New ngApiOpcina
        Dim unserializedContent As Object

        If _parId > -1 Then
            _ApiOpcina.listItem(_parId)
            unserializedContent = _ApiOpcina.List.Item(0)
        ElseIf _parSifra.Length > 0 Then
            _ApiOpcina.listItem(_parSifra)
            unserializedContent = _ApiOpcina.List.Item(0)
        Else
            _ApiOpcina.listItems()
            unserializedContent = _ApiOpcina.List
        End If

        Return Json(unserializedContent)

    End Function

    '
    '   --- Sistematizacija šifarnik ---
    '

    ' API Si-L1
    <HttpGet>
    <Authorize>
    Public Function Sistematizacija() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parId As Integer = -1
        Dim _parSifra As String = ""

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "id"
                    _parId = keypair.Value
                Case "sifra"
                    _parSifra = keypair.Value
            End Select
        Next


        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '
        'Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)

        Dim _ApiSistematizacija = New ngApiSistematizacija
        Dim unserializedContent As Object

        If _parId > -1 Then
            _ApiSistematizacija.listItem(_parId)
            unserializedContent = _ApiSistematizacija.List.Item(0)
        ElseIf _parSifra.Length > 0 Then
            _ApiSistematizacija.listItem(_parSifra)
            unserializedContent = _ApiSistematizacija.List.Item(0)
        Else
            _ApiSistematizacija.listItems()
            unserializedContent = _ApiSistematizacija.List
        End If

        Return Json(unserializedContent)

    End Function

    '
    '   --- Općine šifarnik ---
    '

    ' API Si-L3
    <HttpGet>
    <Authorize>
    Public Function SfrEvidPris() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parVrsta As String = ""

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "vrsta"
                    _parVrsta = keypair.Value
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        _ApiSession.getClaims(Thread.CurrentPrincipal)

        Dim _ApiEvidnecije As New NgApiEvidencije
        _ApiEvidnecije.getSfrEvidPris(_parVrsta)
        Dim unserializedContent = _ApiEvidnecije.SfrEvidPris

        Return Json(unserializedContent)

    End Function



    '
    '   --- EVIDENCIJE - dnevnik rada ---
    '

    ' API ed-L1, L1A, L1B, L1C, L1D, L1E
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <returns></returns>
    <HttpGet>
    <Authorize>
    Public Function EvidDnevnik() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parVrsta As String = ""
        Dim _parEmpID As Integer = -1
        Dim _parMM As Integer = -1
        Dim _parYYYY As Integer = -1
        Dim _parValue As Boolean = False
        Dim _parPrepared As Boolean = True
        Dim _parSendEmail As Boolean = True
        Dim _parSuperLock As Boolean = False

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "vrsta"
                    _parVrsta = keypair.Value
                Case "EmployeeID"
                    _parEmpID = keypair.Value
                Case "mm"
                    _parMM = keypair.Value
                Case "yyyy"
                    _parYYYY = keypair.Value
                Case "value"
                    _parValue = IIf(keypair.Value = "true", True, False)
                Case "prepare"
                    _parPrepared = IIf(keypair.Value = "true", True, False)
                Case "pSendEmail"
                    _parSendEmail = IIf(keypair.Value = "true", True, False)
                Case "pSuperLock"
                    _parSuperLock = IIf(keypair.Value = "true", True, False)
            End Select
        Next

        '
        '   --- Sesija i log ---
        '
        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _ApiEvidDnevnik As New NgApiEvidDnevnik(_parEmpID, _parMM, _parYYYY)

        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' PROVJERA DA LI SE RADI O DNEVNIKU ILI DESKTOP FORMI EVIDENCIJE (Plan/Realizacija)
        '
        If _ApiSession.WorkStation = 1 Then
            ' preuzmi listu iz tabele evd_scheduledata
            _ApiEvidDnevnik.getEvidDnevnikDummy()

        ElseIf _ApiSession.WorkStation = 0 Then

            Select Case _parVrsta
                Case "list"
                    _ApiEvidDnevnik.getEvidDnevnik()
                    '
                    ' Ako nema dnevnika, INSERT
                    '
                    If Not _ApiEvidDnevnik.isCreated Then
                        _ApiEvidDnevnik.insEvidDnevnik(_ApiSession.EmployeeID, _parPrepared)
                        _ApiEvidDnevnik.getEvidDnevnik()
                    End If


    ' API ed-L1A
                Case "send"
                    '
                    ' Pošalji samo ako nije LockExt i EvdKontrolisao
                    '
                    _ApiEvidDnevnik.getEvidDnevnikExt()
                    If _ApiSession.Role = "uposlenik" AndAlso _ApiEvidDnevnik.EvdlockedExt = False _
                         AndAlso _ApiEvidDnevnik.EvdPodnio = 0 Then

                        _ApiEvidDnevnik.setLock(True)
                        _ApiEvidDnevnik.EvdPodnio = _ApiSession.EmployeeID
                        _ApiEvidDnevnik.setLockExt()
                        _ApiEvidDnevnik.del2Evidencije()
                        _ApiEvidDnevnik.ins2Evidencije()


                    ElseIf _ApiSession.Role = "sekretarica" AndAlso _ApiEvidDnevnik.EvdlockedExt = False _
                        AndAlso _ApiEvidDnevnik.EvdKontrolisao > 0 AndAlso _ApiEvidDnevnik.EvdPodnio = 0 Then

                        _ApiEvidDnevnik.setLock(True, _parSuperLock)
                        _ApiEvidDnevnik.EvdPodnio = _ApiSession.EmployeeID
                        _ApiEvidDnevnik.setLockExt()
                        _ApiEvidDnevnik.del2Evidencije()
                        _ApiEvidDnevnik.ins2Evidencije()

                    End If

                    Dim dt As DataTable = _ApiEvidDnevnik.getEvidDnevnik(True)

                    If _parSendEmail AndAlso dt.Rows.Count > 0 Then

                        'Dim _ApiEmails As New NgApiEmail()
                        '_ApiEmails.setSmtpFrom()
                        Dim _ApiEmails As New NgApiEmailNET(_ApiSession)
                        _ApiEmails.setSmtpFrom("info@mperun.net", "Perun - poslovni portal BHRT")

                        Dim _keyValues As New Hashtable()
                        '_keyValues.Add("%__UserFullName__%", _ApiEmails.recEmailtTo.Item(0).name)
                        _keyValues.Add("%__Dnevnik-MMYY__%", _ApiEvidDnevnik.MM.ToString + "-" + _ApiEvidDnevnik.YYYY.ToString)
                        _keyValues.Add("%__Dnevnik-Table__%", _ApiEmails.ConvertDataTableToHTML(dt))

                        'Dim _cc As New List(Of Integer)(New Integer() {1})

                        _ApiEmails.sendByTemplate(_ApiEvidDnevnik.EmployeeID, _keyValues, "email-dnevniksend")

                    End If


    ' API ed-L1B
                Case "unsend"

                    _ApiEvidDnevnik.getEvidDnevnikExt()
                    If _ApiSession.Role = "uposlenik" AndAlso _ApiEvidDnevnik.EvdlockedExt = False _
                        AndAlso _ApiEvidDnevnik.EvdPodnio > 0 AndAlso _ApiEvidDnevnik.EvdKontrolisao = 0 Then
                        _ApiEvidDnevnik.setLock(False)
                        _ApiEvidDnevnik.EvdPodnio = 0
                        _ApiEvidDnevnik.setLockExt()
                        _ApiEvidDnevnik.del2Evidencije()

                    ElseIf _ApiSession.Role = "sekretarica" AndAlso _ApiEvidDnevnik.EvdlockedExt = False _
                    AndAlso _ApiEvidDnevnik.EvdPodnio > 0 AndAlso _ApiEvidDnevnik.EvdKontrolisao > 0 Then
                        _ApiEvidDnevnik.setLock(False, _parSuperLock)
                        _ApiEvidDnevnik.EvdPodnio = 0
                        _ApiEvidDnevnik.setLockExt()
                        _ApiEvidDnevnik.del2Evidencije()

                    End If

                    _ApiEvidDnevnik.getEvidDnevnik()

                    If _parSendEmail Then

                        'Dim _ApiEmails As New NgApiEmail()
                        '_ApiEmails.setSmtpFrom()
                        Dim _ApiEmails As New NgApiEmailNET(_ApiSession)
                        _ApiEmails.setSmtpFrom("info@mperun.net", "Perun - poslovni portal BHRT")

                        Dim _keyValues As New Hashtable()
                        _keyValues.Add("%__Dnevnik-MMYY__%", _ApiEvidDnevnik.MM.ToString + "-" + _ApiEvidDnevnik.YYYY.ToString)

                        _ApiEmails.sendByTemplate(_ApiEvidDnevnik.EmployeeID, _keyValues, "email-dnevnikunsend")

                    End If

    ' API ed-L1C
                Case "evunos"
                    _ApiEvidDnevnik.getEvidDnevnikExt()
                    If _ApiSession.Role = "sekretarica" AndAlso _ApiEvidDnevnik.EvdKontrolisao > 0 _
                        AndAlso _ApiEvidDnevnik.EvdlockedExt = False Then
                        _ApiEvidDnevnik.EvdPodnio = 0
                        _ApiEvidDnevnik.setLockExt()
                    End If

                    _ApiEvidDnevnik.getEvidDnevnik()

                    If _parSendEmail Then

                        'Dim _ApiEmails As New NgApiEmail()
                        '_ApiEmails.setSmtpFrom()
                        Dim _ApiEmails As New NgApiEmailNET(_ApiSession)
                        _ApiEmails.setSmtpFrom("info@mperun.net", "Perun - poslovni portal BHRT")

                        Dim _keyValues As New Hashtable()
                        _keyValues.Add("%__Dnevnik-MMYY__%", _ApiEvidDnevnik.MM.ToString + "-" + _ApiEvidDnevnik.YYYY.ToString)

                        '_ApiEmails.sendByTemplate(_ApiEvidDnevnik.EmployeeID, _keyValues, "email-dnevnikkontstart")

                    End If

    ' API ed-L1D
                Case "evstop"
                    _ApiEvidDnevnik.getEvidDnevnikExt()
                    If _ApiSession.Role = "sekretarica" AndAlso _ApiEvidDnevnik.EvdlockedExt = False Then
                        Dim _EvdKontrolisao As Integer = IIf(_parValue = True, _ApiSession.EmployeeID, 0)
                        _ApiEvidDnevnik.EvdKontrolisao = _EvdKontrolisao
                        _ApiEvidDnevnik.setLockExt()
                    End If

                    _ApiEvidDnevnik.getEvidDnevnik()

                    If _parSendEmail Then

                        'Dim _ApiEmails As New NgApiEmail()
                        '_ApiEmails.setSmtpFrom()
                        Dim _ApiEmails As New NgApiEmailNET(_ApiSession)
                        _ApiEmails.setSmtpFrom("info@mperun.net", "Perun - poslovni portal BHRT")

                        Dim _keyValues As New Hashtable()
                        _keyValues.Add("%__Dnevnik-MMYY__%", _ApiEvidDnevnik.MM.ToString + "-" + _ApiEvidDnevnik.YYYY.ToString)

                        ' _ApiEmails.sendByTemplate(_ApiEvidDnevnik.EmployeeID, _keyValues, "email-dnevnikkontstop")

                    End If

    ' API ed-L1E
                Case "evlock"
                    _ApiEvidDnevnik.getEvidDnevnikExt()
                    If _ApiSession.Role = "sekretarica" AndAlso _ApiEvidDnevnik.EvdKontrolisao > 0 Then
                        _ApiEvidDnevnik.EvdlockedExt = _parValue
                        _ApiEvidDnevnik.setLockExt()
                    End If

                    _ApiEvidDnevnik.getEvidDnevnik()

            End Select

        End If







        Dim unserializedContent = _ApiEvidDnevnik.EvidDnevnik
        Return Json(unserializedContent)

    End Function


    ' API ed-S1
    <HttpPost>
    <Authorize>
    Public Function EvidDnevnikWrite(<FromBody()> ByVal valuePost As List(Of NgEvidDnevnik)) As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()
        Dim _parSuperLock As Boolean = False

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "pSuperLock"
                    _parSuperLock = IIf(keypair.Value = "true", True, False)
            End Select
        Next


        '
        '   --- Sesija i log ---
        '
        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _ApiEvidDnevnik As New NgApiEvidDnevnik()

        Dim _lstUpd As New List(Of NgEvidDnevnik)
        Dim _lstDel As New List(Of NgEvidDnevnik)
        Dim _lstIns As New List(Of NgEvidDnevnik)

        For Each el As NgEvidDnevnik In valuePost

            If el.vrijeme_od.Contains(":") Then
                el.vrijeme_od = el.vrijeme_od + ":00"
            End If
            If el.vrijeme_do.Contains(":") Then
                el.vrijeme_do = el.vrijeme_do + ":00"
            End If
            'If el.vrijeme_ukupno.Contains(":") Then
            '    el.vrijeme_ukupno = el.vrijeme_ukupno + ":00"
            'End If
            _ApiEvidDnevnik.EmployeeID = el.employeeID
            _ApiEvidDnevnik.MM = Month(el.datum)
            _ApiEvidDnevnik.YYYY = Year(el.datum)



            Select Case el.id
                Case > 0
                    _lstUpd.Add(el)
                Case = -1
                    _lstIns.Add(el)
                Case < -1
                    el.id = -el.id
                    _lstDel.Add(el)
            End Select
        Next


        _ApiEvidDnevnik.getEvidDnevnikExt()
        '
        ' Uposlenik - Spasi
        '
        If _ApiSession.Role = "uposlenik" AndAlso
           _ApiEvidDnevnik.EvdlockedExt = False AndAlso
           _ApiEvidDnevnik.EvdPodnio = 0 Then

            If _lstUpd.Count() > 0 Then
                _ApiEvidDnevnik.updEvidDnevnikRow(_ApiSession.EmployeeID, _lstUpd)
            End If

            If _lstIns.Count() > 0 Then
                _ApiEvidDnevnik.insEvidDnevnikRow(_ApiSession.EmployeeID, _lstIns)
            End If

            If _lstDel.Count() > 0 Then
                _ApiEvidDnevnik.delEvidDnevnikRow(_lstDel)
            End If

        End If

        '
        ' Ostale role - Spasi
        '
        If _ApiSession.Role = "sekretarica" AndAlso
            _ApiEvidDnevnik.EvdlockedExt = False AndAlso
            _ApiEvidDnevnik.EvdPodnio = 0 Then

            If _lstUpd.Count() > 0 Then
                _ApiEvidDnevnik.updEvidDnevnikRow(_ApiSession.EmployeeID, _lstUpd, _parSuperLock)
            End If

            If _lstIns.Count() > 0 Then
                _ApiEvidDnevnik.insEvidDnevnikRow(_ApiSession.EmployeeID, _lstIns)
            End If

            If _lstDel.Count() > 0 Then
                _ApiEvidDnevnik.delEvidDnevnikRow(_lstDel)
            End If

        End If


        Dim _parEmpID As Integer = -1
        Dim _parMM As Integer = -1
        Dim _parYYYY As Integer = -1

        If valuePost.Count() > 0 Then
            _parEmpID = valuePost(0).employeeID
            Dim _dt As DateTime = DateTime.Parse(valuePost(0).datum)
            _parMM = _dt.Month
            _parYYYY = _dt.Year
        End If


        _ApiEvidDnevnik = New NgApiEvidDnevnik(_parEmpID, _parMM, _parYYYY)
        _ApiEvidDnevnik.getEvidDnevnik()

        Dim unserializedContent = _ApiEvidDnevnik.EvidDnevnik
        Return Json(unserializedContent)

    End Function


    ' API go-L1
    <HttpGet>
    <Authorize>
    Public Function EvidGodOdm() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parEmpID As Integer = -1
        Dim _parYYYY As Integer = -1

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "EmployeeID"
                    _parEmpID = keypair.Value
                Case "yyyy"
                    _parYYYY = keypair.Value
            End Select
        Next

        '
        '   --- Sesija i log ---
        '
        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _ApiEvidGododm As New NgApiEvidGodOdm(_parEmpID, _parYYYY)
        _ApiEvidGododm.getEvidGodOdm()

        Dim unserializedContent = _ApiEvidGododm.EvidGodOdm
        Return Json(unserializedContent)

    End Function

    ' API ed-L2
    <HttpGet>
    <Authorize>
    Public Function EvidDnStatusByOrgRole() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parList As String = ""
        Dim _parMM As Integer = -1
        Dim _parYYYY As Integer = -1

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "list"
                    _parList = keypair.Value
                Case "mm"
                    Integer.TryParse(keypair.Value, _parMM)
                Case "yyyy"
                    Integer.TryParse(keypair.Value, _parYYYY)
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _ngApiOrganizacija As New NgApiOrganizacija
        Dim _ApiEvidDnevnik As New NgApiEvidDnevnik(_parMM, _parYYYY)

        Dim _orgIds As String = ""
        Dim _PGIds As String = ""

        Select Case _parList
            Case "byorg"
                Dim _roleOrgIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email)
                '
                ' Uzmi rekurzivno sve org id u hijerarhiji organizacije
                _ngApiOrganizacija.GetOrgSifra(_roleOrgIds, 5, 1)
                _orgIds = String.Join(",", _ngApiOrganizacija.lstOrgSifras)

            Case "bypg"
                Dim _rolePGIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email, True)
                _ngApiOrganizacija.GetPGEmpIds(_rolePGIds)
                _PGIds = String.Join(",", _ngApiOrganizacija.lstEmpIds)

            Case "byorgpg"
                ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
                '
                Dim _roleOrgIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email)
                '
                ' Uzmi rekurzivno sve org id u hijerarhiji organizacije
                _ngApiOrganizacija.GetOrgSifra(_roleOrgIds, 5, 1)
                _orgIds = String.Join(",", _ngApiOrganizacija.lstOrgSifras)

                Dim _rolePGIds As String = _ngApiOrganizacija.getOrgRole(_ApiSession.Email, True)
                _ngApiOrganizacija.GetPGEmpIds(_rolePGIds)
                _PGIds = String.Join(",", _ngApiOrganizacija.lstEmpIds)

        End Select

        _orgIds = IIf(_orgIds.Length > 0, _orgIds, "")
        _PGIds = IIf(_PGIds.Length > 0, _PGIds, "-1")


        Dim unserializedContent = _ApiEvidDnevnik.getEvidDnStatusByOrgPG(_orgIds, _PGIds)
        Return Json(unserializedContent)

    End Function

    ' API ed-S2
    <HttpPost>
    <Authorize>
    Public Function EvidDnStatusWrite(<FromBody()> ByVal valueEvidDnStatusLst As List(Of NgEvidDnStatus)) As IHttpActionResult
        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parSendEmail As Boolean = True

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "pSendEmail"
                    _parSendEmail = IIf(keypair.Value = "true", True, False)
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _ngEvidDnStatusLst As List(Of NgEvidDnStatus) = valueEvidDnStatusLst

        Dim _ApiEvidDnevnik As NgApiEvidDnevnik
        _ApiEvidDnevnik = New NgApiEvidDnevnik()

        _ApiEvidDnevnik.updEvidDnevnikStatus(_ngEvidDnStatusLst)

        If _parSendEmail Then

            'Dim _ApiEmails As New NgApiEmail()
            '_ApiEmails.setSmtpFrom()
            Dim _ApiEmails As New NgApiEmailNET(_ApiSession)
            _ApiEmails.setSmtpFrom("info@mperun.net", "Perun - poslovni portal BHRT")

            Dim _keyValues As New Hashtable()

            For Each el In _ngEvidDnStatusLst

                _keyValues.Clear()
                _keyValues.Add("%__Dnevnik-MMYY__%", el.MM.ToString + "-" + el.YYYY.ToString)

                Try
                    If el.evd_kontrolisao > 0 AndAlso el.evd_podnio = 0 Then
                        _ApiEmails.sendByTemplate(el.employeeID, _keyValues, "email-dnevnikkontstart")
                    Else
                        '_ApiEmails.sendByTemplate(el.employeeID, _keyValues, "email-dnevnikkontstop")
                    End If
                Catch ex As Exception

                End Try



            Next


        End If

        Dim unserializedContent = _ApiEvidDnevnik.getEvidDnStatusById(_ngEvidDnStatusLst)

        Return Json(unserializedContent)

    End Function

    <HttpGet>
    <Authorize>
    Public Function EvidSuma() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parSifra As String = ""
        Dim _parEmpID As Integer = -1
        Dim _parMM As Integer = -1
        Dim _parYYYY As Integer = -1
        Dim _parValue As Boolean = False

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "sifra"
                    _parSifra = keypair.Value
                Case "EmployeeID"
                    _parEmpID = keypair.Value
                Case "mm"
                    _parMM = keypair.Value
                Case "yyyy"
                    _parYYYY = keypair.Value
                Case "value"
                    _parValue = IIf(keypair.Value = "true", True, False)
            End Select
        Next

        '
        '   --- Sesija i log ---
        '
        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        'Dim _ApiEvidDnevnik As New NgApiEvidDnevnik(_parEmpID, _parMM, _parYYYY)
        Dim _ApiSumaEvidencije As New ClsSumaEvidencije()

        '_ApiEvidDnevnik.getEvidDnevnik()
        '
        ' Ako nema dnevnika, INSERT
        '
        _ApiSumaEvidencije.TableName = _parSifra
        _ApiSumaEvidencije.LoadDbEvid(_parMM, _parYYYY)

        Dim destinationTable As New DataTable
        destinationTable = _ApiSumaEvidencije.DataSet.Tables(_ApiSumaEvidencije.TableName)
        destinationTable.Columns.Remove("R500")


        Dim mmGG As String = _parMM.ToString + "-" + _parYYYY.ToString
        Dim _ApiDocument As New ngApiDocument()

        Dim _rptRetFilename As String = _ApiDocument.createPdf(destinationTable, mmGG, _ApiSumaEvidencije.TableName)


        Dim unserializedContent = _rptRetFilename
        Return Json(unserializedContent)

    End Function

    '
    ' EMAIL
    '

    ' API Em-C1
    <HttpGet>
    <Authorize>
    Public Function sendEmail() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parEmployeeId As Integer = -1
        Dim _parTemplate As String = ""
        Dim _parSubject As String = ""

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "pEmployeeId"
                    _parEmployeeId = keypair.Value
                Case "pSubject"
                    _parSubject = keypair.Value
                Case "pTemplate"
                    _parTemplate = keypair.Value
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)

        '  
        ' TODO: Uvesti parametar koji će kroz POST prenijeti sve parametre koji su potrebni za template:
        '               -Subject
        '               -%__UserFullName__%
        '               ...

        'Dim _ApiEmails As New NgApiEmail()
        '_ApiEmails.setSmtpFrom()
        Dim _ApiEmails As New NgApiEmailNET(_ApiSession)
        _ApiEmails.setSmtpFrom("info@mperun.net", "Perun - poslovni portal BHRT")

        Dim _ApiEmployee As New NgApiEmployee()
        Dim _employee As NgEmployee = _ApiEmployee.listItem(_parEmployeeId)

        _ApiEmails.set1MailTo(_parEmployeeId, _parSubject)

        Dim _body As String
        '_body = _ApiEmails.getSmtpTemplateById(2)
        _body = _ApiEmails.getSmtpTemplateFromFile(_parTemplate)

        _body = _body.Replace("%__UserFullName__%", _ApiEmails.recEmailtTo.Item(0).name)


        'Dim unserializedContent = _ApiEmails.SendOneMail(_body)

        _ApiEmails.emailBody = _body
        Dim unserializedContent = _ApiEmails.SendMail()

        Return Json(unserializedContent)

    End Function

    ' API Em-C1
    <HttpGet>
    <Authorize>
    Public Function sendEmail2PG() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parPG As Integer = -1
        Dim _parTemplate As String = ""
        Dim _parSubject As String = ""

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "pPG"
                    _parPG = keypair.Value
                Case "pSubject"
                    _parSubject = keypair.Value
                Case "pTemplate"
                    _parTemplate = keypair.Value
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)

        '  
        ' TODO: Uvesti parametar koji će kroz POST prenijeti sve parametre koji su potrebni za template:
        '               -Subject
        '               -%__UserFullName__%
        '               ...

        'Dim _ApiEmails As New NgApiEmail()
        '_ApiEmails.setSmtpFrom()
        Dim _ApiEmails As New NgApiEmailNET(_ApiSession)
        _ApiEmails.setSmtpFrom("info@mperun.net", "Perun - poslovni portal BHRT")


        Dim _ApiEmployee As New NgApiEmployee()
        Dim _empIds = _ApiEmployee.listItems_PG(_parPG)
        Dim _body As String
        Dim _retBool = True

        For Each el In _empIds
            _ApiEmails.recEmailtTo.Clear()
            _ApiEmails.set1MailTo(el, _parSubject)

            _body = _ApiEmails.getSmtpTemplateFromFile(_parTemplate)
            _body = _body.Replace("%__UserFullName__%", _ApiEmails.recEmailtTo.Item(0).name)

            _ApiEmails.emailBody = _body
            _retBool = _retBool And _ApiEmails.SendMail()

        Next



        Return Json(_retBool)

    End Function

    ' API Em-C2
    <HttpPost>
    <Authorize>
    Public Function sendEmail(<FromBody()> ByVal valuePost As List(Of NgEmployee)) As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parTemplate As String = ""

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "pTemplate"
                    _parTemplate = keypair.Value
            End Select
        Next



        '
        '   --- Sesija i log ---
        '
        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        'Dim _ApiEmails As New NgApiEmail()
        '_ApiEmails.setSmtpFrom()
        Dim _ApiEmails As New NgApiEmailNET(_ApiSession)
        _ApiEmails.setSmtpFrom("info@mperun.net", "Perun - poslovni portal BHRT")

        For Each el As NgEmployee In valuePost

            Dim _ApiEmployee As New NgApiEmployee()
            Dim _employee As NgEmployee = _ApiEmployee.listItem(el.EmployeeID)

            Dim _empEmail As String = IIf(_employee.emp_work_default_email = 1, _employee.emp_work_email, _employee.emp_oth_email)
            Dim _empName As String = _employee.FirstName + " " + _employee.LastName

            '_ApiEmails.emailTo.Add(_empEmail)
            '_ApiEmails.emailName.Add(_empName)

            _ApiEmails.recEmailtTo.Add(New NgEmaillAddress(_empEmail, _empName))

            Dim _body As String
            '_body = _ApiEmails.getSmtpTemplateById(2)
            _body = _ApiEmails.getSmtpTemplateFromFile(_parTemplate)

            _body = _body.Replace("%__UserFullName__%", _empName)
            _body = _body.Replace("%__UserName__%", _employee.emp_username)
            _body = _body.Replace("%__UserPass__%", "bhrt" + el.EmployeeID.ToString)
            _ApiEmails.emailBodies.Add(_body)

        Next

        Dim _subject As String = "Kreiran pristup portalu"
        Dim unserializedContent = _ApiEmails.SendMail(_subject)

        Return Json(unserializedContent)

    End Function



    ' API L1
    <HttpGet>
    <Authorize>
    Public Function Menus() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _ApiMenusByRole As New NgApiMenusByRole()
        _ApiMenusByRole.getMenu(_ApiSession.Role, _ApiSession.Email)

        Dim unserializedContent = _ApiMenusByRole.MenusByRoleList

        Return Json(unserializedContent)

    End Function




    '
    '  EVIDENCIJA JWT
    '


    <HttpGet>
    <Authorize>
    Public Function SfrEvidPris2() As IHttpActionResult

        Dim _ApiEvidnecije As New NgApiEvidencije
        Dim unserializedContent = _ApiEvidnecije.SfrEvidPris

        Return Json(unserializedContent)

    End Function

    <HttpGet>
    <Authorize>
    Public Function EvidPris(ByVal pEmpID As Integer, ByVal pYear As Integer, ByVal pMonth As Integer, Optional ByVal pFormat As Integer = 0) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '

        If pEmpID = -1 Then pEmpID = _ApiSession.EmployeeID

        Dim _ApiEvidencije As New NgApiEvidencije
        Dim unserializedContent As Object = Nothing

        Select Case pFormat
            '<<<<< tabela evd_prisustva output: NgEvidDnevnik
            Case 0
                _ApiEvidencije.getEvidPris(pEmpID, pYear, pMonth)
                unserializedContent = _ApiEvidencije.EvidPris

            Case 1
                '<<<<< tabela evd_prisustva output: NgEvidDnevnik DNEVNIK
                unserializedContent = _ApiEvidencije.getEvidPris1(pEmpID, pYear, pMonth)

            Case 2
                '<<<<< tabela evd_scheddnevnik output: NgEvidDnevnik DNEVNIK
                Dim _ApiEvidDnevnik As New NgApiEvidDnevnik(pEmpID, pMonth, pYear)
                Dim _parPrepared As Boolean = True

                If _ApiSession.WorkStation = 1 Then
                    ' preuzmi listu iz tabele evd_scheduledata
                    _ApiEvidDnevnik.getEvidDnevnikDummy()

                ElseIf _ApiSession.WorkStation = 0 Then
                    _ApiEvidDnevnik.getEvidDnevnik()
                    '
                    ' Ako nema dnevnika, INSERT
                    '
                    If Not _ApiEvidDnevnik.isCreated Then
                        _ApiEvidDnevnik.insEvidDnevnik(_ApiSession.EmployeeID, _parPrepared)
                        _ApiEvidDnevnik.getEvidDnevnik()
                    End If
                End If

                unserializedContent = _ApiEvidDnevnik.EvidDnevnik

        End Select





        Return Json(unserializedContent)

    End Function

    <HttpGet>
    <Authorize>
    Public Function EvidPrisStatus(ByVal pEmpID As Integer, ByVal pYear As Integer, ByVal pMonth As Integer, Optional ByVal pFormat As Integer = 0) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '

        If pEmpID = -1 Then pEmpID = _ApiSession.EmployeeID

        Dim _ApiEvidencije As New NgApiEvidencije
        Dim unserializedContent As Object = Nothing

        Select Case pFormat
            '<<<<< tabela evid_prisustva output: NgEvidPrisStatus
            Case 0
                _ApiEvidencije.getEvidPrisStatus(pEmpID, pYear, pMonth)

                If _ApiSession.WorkStation = 1 Then
                    _ApiEvidencije.EvidPrisStatus.allowEdit = False
                    _ApiEvidencije.EvidPrisStatus.locked = True
                End If
                unserializedContent = _ApiEvidencije.EvidPrisStatus

            Case 1
                '<<<<< tabela evid_prisustva output: NgEvidDnStatus DNEVNIK
                unserializedContent = _ApiEvidencije.getEvidPrisStatus1(pEmpID, pYear, pMonth)

            Case 2
                '<<<<< tabela evid_prisustva output: NgEvidDnStatus DNEVNIK
                unserializedContent = _ApiEvidencije.getEvidPrisStatus1(pEmpID, pYear, pMonth)

        End Select



        Return Json(unserializedContent)

    End Function

    <HttpPost>
    <Authorize>
    Public Function EvidPrisWrite(
        <FromBody> ByVal pEvidPrisTbl As List(Of NgEvidDnevnnik)) As IHttpActionResult


        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)

        Dim _ApiEvidencije As New NgApiEvidencije


        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '

        _ApiEvidencije.insEvidPris(pEvidPrisTbl)


        '_ApiEvidnecije.getEvidPris(pEmpID, pYear, pMonth)
        Dim unserializedContent = _ApiEvidencije.EvidPris

        Return Json(unserializedContent)

    End Function

    '
    '<<<<< tabela evid_prisustva output: NgEvidDnevnik DNEVNIK
    '
    <HttpPost>
    <Authorize>
    Public Function EvidPrisWrite1(
        <FromBody> ByVal pEvidPrisTbl As List(Of NgEvidDnevnik)) As IHttpActionResult


        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)

        Dim _ApiEvidencije As New NgApiEvidencije


        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked
        '  
        '

        Dim unserializedContent = _ApiEvidencije.insEvidPris1(pEvidPrisTbl)


        Return Json(unserializedContent)

    End Function

    ' API ed-S1
    <HttpPost>
    <Authorize>
    Public Function EvidPrisWrite2(
                                  <FromBody()> ByVal valuePost As List(Of NgEvidDnevnik)) As IHttpActionResult


        '
        '   --- Sesija i log ---
        '
        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _ApiEvidDnevnik As New NgApiEvidDnevnik()

        Dim _lstUpd As New List(Of NgEvidDnevnik)
        Dim _lstDel As New List(Of NgEvidDnevnik)
        Dim _lstIns As New List(Of NgEvidDnevnik)

        For Each el As NgEvidDnevnik In valuePost

            If el.vrijeme_od.Contains(":") Then
                el.vrijeme_od = el.vrijeme_od + ":00"
            End If
            If el.vrijeme_do.Contains(":") Then
                el.vrijeme_do = el.vrijeme_do + ":00"
            End If
            'If el.vrijeme_ukupno.Contains(":") Then
            '    el.vrijeme_ukupno = el.vrijeme_ukupno + ":00"
            'End If
            _ApiEvidDnevnik.EmployeeID = el.employeeID
            _ApiEvidDnevnik.MM = Month(el.datum)
            _ApiEvidDnevnik.YYYY = Year(el.datum)



            Select Case el.id
                Case > 0
                    _lstUpd.Add(el)
                Case = -1
                    _lstIns.Add(el)
                Case < -1
                    el.id = -el.id
                    _lstDel.Add(el)
            End Select
        Next


        _ApiEvidDnevnik.getEvidDnevnikExt()
        '
        ' Uposlenik - Spasi
        '
        If _ApiSession.Role = "uposlenik" AndAlso
           _ApiEvidDnevnik.EvdlockedExt = False AndAlso
           _ApiEvidDnevnik.EvdPodnio = 0 Then

            If _lstUpd.Count() > 0 Then
                _ApiEvidDnevnik.updEvidDnevnikRow(_ApiSession.EmployeeID, _lstUpd)
            End If

            If _lstIns.Count() > 0 Then
                _ApiEvidDnevnik.insEvidDnevnikRow(_ApiSession.EmployeeID, _lstIns)
            End If

            If _lstDel.Count() > 0 Then
                _ApiEvidDnevnik.delEvidDnevnikRow(_lstDel)
            End If

        End If



        Dim _parEmpID As Integer = -1
        Dim _parMM As Integer = -1
        Dim _parYYYY As Integer = -1

        If valuePost.Count() > 0 Then
            _parEmpID = valuePost(0).employeeID
            Dim _dt As DateTime = DateTime.Parse(valuePost(0).datum)
            _parMM = _dt.Month
            _parYYYY = _dt.Year
        End If


        _ApiEvidDnevnik = New NgApiEvidDnevnik(_parEmpID, _parMM, _parYYYY)
        _ApiEvidDnevnik.getEvidDnevnik()

        Dim unserializedContent = _ApiEvidDnevnik.EvidDnevnik
        Return Json(unserializedContent)

    End Function

    <HttpGet>
    <Authorize>
    Public Function EvidPrisStatusWrite(ByVal pEmpID As Integer, ByVal pYear As Integer, ByVal pMonth As Integer, ByVal pPodnioEmpID As Integer) As IHttpActionResult


        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)

        ' Provjere:
        '  1 - ima li pravo da vidi podatke uposlenika (Organizacijska rola)
        '  2 - Uzeti status: podnio, odobrio, kontrolisao, locked


        If pEmpID = -1 Then pEmpID = _ApiSession.EmployeeID

        Dim _ApiEvidencije As New NgApiEvidencije
        Dim retVal As Boolean = _ApiEvidencije.updEvidPrisPodnioStatus(pEmpID, pYear, pMonth, pPodnioEmpID)


        _ApiEvidencije.getEvidPrisStatus(pEmpID, pYear, pMonth)
        Dim unserializedContent = _ApiEvidencije.EvidPrisStatus

        Return Json(unserializedContent)

    End Function

    '
    ' Geolocation API - GL-L1, GL-S1
    '

    ' API GL-L1
    <HttpGet>
    <Authorize>
    Public Function GeoLocStatus() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parEmpId As Integer = -1
        Dim _parNumRec As Integer = 1


        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "EmpId"
                    _parEmpId = Integer.Parse(keypair.Value)
                Case "NumRec"
                    _parNumRec = Integer.Parse(keypair.Value)
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)



        Dim _EvidPrisDolaska As New NgEvidPrisDolaska
        _EvidPrisDolaska.EmployeeId = _parEmpId

        ' Vrati zadnjih 10 redova iz tabele
        '
        Dim _ApiApiEvidPrisDolaska As New ngApiEvidPrisDolaska()
        Dim unserializedContent = _ApiApiEvidPrisDolaska.selEvidPrisRec(_EvidPrisDolaska, _parNumRec)


        Return Json(unserializedContent)

    End Function

    ' API GL-S1
    <HttpGet>
    <Authorize>
    Public Function GeoLocPrijava() As IHttpActionResult

        Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()

        Dim _parEmpId As Integer = -1
        Dim _parLat As String = ""
        Dim _parLong As String = ""
        Dim _parNapomena As String = ""

        For Each keypair As KeyValuePair(Of String, String) In _allUrlKeyValues
            Select Case keypair.Key
                Case "EmpId"
                    _parEmpId = Integer.Parse(keypair.Value)
                Case "Lat"
                    _parLat = keypair.Value
                Case "Long"
                    _parLong = keypair.Value
                Case "Napomena"
                    _parNapomena = keypair.Value
            End Select
        Next

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '
        Dim _className As String = Me.GetType().Name
        Dim _methodName As String = MethodBase.GetCurrentMethod().Name

        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": " + _className + "." + _methodName
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _EvidPrisDolaska As New NgEvidPrisDolaska
        _EvidPrisDolaska.EmployeeId = _parEmpId

        ' Provjeri da li je na lokaciji
        'Dim _ApiApiEvidPrisDolaska As New ngApiEvidPrisDolaska("PolygonPoints.XML")

        Dim _ApiApiEvidPrisDolaska As New ngApiEvidPrisDolaska(_EvidPrisDolaska)
        Dim _onLocation = _ApiApiEvidPrisDolaska.checkGeoLoc(_parLat, _parLong)


        ' Provjeri da li je izvršena prijava Y/N
        ' 
        _EvidPrisDolaska = _ApiApiEvidPrisDolaska.selEvidPrisIN(_EvidPrisDolaska)

        ' Procesiranje prijave: in, out, 
        Dim _napomena As String = ""

        Select Case _EvidPrisDolaska.OnLocation

            Case False
                ' Ako je N (False) i _onLocation = TRUE, upiši podatke o ulasku
                ' insIN
                If _onLocation = True Then
                    _EvidPrisDolaska.InGeoloc = _parLat + ", " + _parLong
                    _EvidPrisDolaska.OnLocation = _onLocation

                    _ApiApiEvidPrisDolaska.insEvidPrisIn(_EvidPrisDolaska)
                    _napomena = "prijava - korisnik na lokaciji"
                Else
                    _napomena = "nema prijave - korisnik van lokacije"
                End If


            Case True
                ' Ako je Y i _onLocation = TRUE, upiši podatke o izlasku na lokaciji
                ' updOUT

                ' Ako je Y i _onLocation = FALSE, upiši podatke o izlasku van lokacije
                ' updOUT
                If _parNapomena.Length > 0 Then
                    _EvidPrisDolaska.Opis = _parNapomena + " : "
                End If

                If _onLocation = True Then
                    _EvidPrisDolaska.Opis += "odjava na lokaciji"
                    _napomena = "odjava - korisnik na lokaciji"
                Else
                    _EvidPrisDolaska.Opis += "odjava van lokacije"
                    _napomena = "odjava - korisnik van lokacije"
                End If

                _EvidPrisDolaska.OutGeoloc = _parLat + ", " + _parLong
                _EvidPrisDolaska.OnLocation = False

                _ApiApiEvidPrisDolaska.insEvidPrisOut(_EvidPrisDolaska)
        End Select


        ' Vrati red iz sql tabele evd_pris_dolaska
        Dim unserializedContent As List(Of NgEvidPrisDolaska) = _ApiApiEvidPrisDolaska.selEvidPrisRec(_EvidPrisDolaska)
        unserializedContent.Item(0).Napomena = _napomena

        Return Json(unserializedContent)

    End Function

    <HttpGet>
    Public Function getAPIver() As IHttpActionResult


        Dim _ngApiVersion = New ngApiVersion()

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' Insert LOG
        '
        Dim _ApiSession As New NgApiSession

        Dim _log As String = _ngApiVersion.getFooter()
        Dim _comment As String = _ngApiVersion.getMainText()
        _ApiSession.insApiLog(_log, _comment)


        Return Json(_ngApiVersion)

    End Function


    '''''''''''''''
    ' API saradnici
    '''''''''''''''

    <HttpPost>
    <Authorize>
    Public Function person(<FromBody()> ByVal entity As ngPerson) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        entity.Insert()

        Return Json(entity)

    End Function

    <HttpPut>
    <Authorize>
    Public Function person(ByVal pSifra As Integer, <FromBody()> ByVal entity As ngPerson) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim request = HttpContext.Current.Request

        Dim responsedata As Stream = request.InputStream
        Dim responsereader As StreamReader = New StreamReader(responsedata)
        Dim xResponse = responsereader.ReadToEnd

        Dim reader As New JsonTextReader(New StringReader(xResponse))
        reader.SupportMultipleContent = True

        Dim serializer As New JsonSerializer()
        Dim _person As ngPerson = serializer.Deserialize(Of ngPerson)(reader)

        Dim _personQry = New ngPersonQuery()
        Dim _personResult = _personQry.FindOne(pSifra)

        If _personResult Is Nothing Then
            Return NotFound()
        End If

        Dim _existSifra As Boolean = False
        Try
            _existSifra = xResponse.ToLower().Contains("sifra")
            If _existSifra = True AndAlso _person.Sifra = 0 Then
                _personResult.Sifra = 0
            End If
        Catch ex As Exception

        End Try

        _personResult.Ime = If(_person.Ime Is Nothing, _personResult.Ime, _person.Ime)
        _personResult.ImeRoditelja = If(_person.ImeRoditelja Is Nothing, _personResult.ImeRoditelja, _person.ImeRoditelja)
        _personResult.Prezime = If(_person.Prezime Is Nothing, _personResult.Prezime, _person.Prezime)
        _personResult.KategorijaUposlenika = If(_person.KategorijaUposlenika Is Nothing, _personResult.KategorijaUposlenika, _person.KategorijaUposlenika)
        _personResult.JMBG = If(_person.JMBG Is Nothing, _personResult.JMBG, _person.JMBG)
        _personResult.Spol = If(_person.Spol Is Nothing, _personResult.Spol, _person.Spol)
        _personResult.Status = If(_person.Status Is Nothing, _personResult.Status, _person.Status)

        _personResult.Update()

        Return Json(_personResult)

    End Function

    <HttpDelete>
    <Authorize>
    Public Function person(ByVal pSifra As String) As IHttpActionResult

        Dim _sifra As Integer = pSifra

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _personQry = New ngPersonQuery()
        Dim _personResult = _personQry.FindOne(_sifra)

        If _personResult Is Nothing Then
            Return NotFound()
        End If

        _personResult.Delete()

        Return Ok()

    End Function


    <HttpGet>
    <Authorize>
    Public Function person(ByVal pSifra As Integer) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _personQry = New ngPersonQuery()
        Dim _personResult = _personQry.FindOne(pSifra)

        If _personResult Is Nothing Then
            Return NotFound()
        End If

        Return Json(_personResult)

    End Function

    <HttpGet>
    <Authorize>
    Public Function person() As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _person = New ngPersonQuery()
        Dim _personLst = _person.AllPersons()

        Return Json(_personLst)

    End Function

    <HttpGet>
    <Authorize>
    Public Function personunion() As IHttpActionResult
        'Dim _allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs()


        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        ' Uzmi lstOrg i lstPG iz sys_usr_role_json 
        '
        Dim _ngApiOrganizacija As New NgApiOrganizacija
        Dim _roleOrgIds As String = ""
        Dim _rolePGIds As String = ""

        _ngApiOrganizacija.getOrgRole(_ApiSession.Email, _roleOrgIds, _rolePGIds)

        Dim _regExpOrgSfr As String = _ngApiOrganizacija.sqlPrepRegExpOrg(_roleOrgIds)

        _ngApiOrganizacija.GetPGEmpIds(_rolePGIds)
        Dim _PGIds As String = String.Join(",", _ngApiOrganizacija.lstEmpIds)

        Dim _PGExt As String = _ngApiOrganizacija.GetPGExterni(_rolePGIds)

        Dim _personLst = ngPersonUnionQuery.AllPersons2(_regExpOrgSfr, _PGIds, _PGExt)

        Return Json(_personLst)

    End Function



    '''''''''''''''
    ' API ProtokolEvidencija
    '''''''''''''''

    <HttpPost>
    <Authorize>
    Public Function pn_protokolevid(<FromBody()> ByVal entity As ngPNProtokolEvidencija) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        entity.Insert()

        Return Json(entity)

    End Function

    <HttpPut>
    <Authorize>
    Public Function pn_protokolevid(ByVal pPnId As Integer, <FromBody()> ByVal entity As ngPNProtokolEvidencija) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim request = HttpContext.Current.Request

        Dim responsedata As Stream = request.InputStream
        Dim responsereader As StreamReader = New StreamReader(responsedata)
        Dim xResponse = responsereader.ReadToEnd

        Dim reader As New JsonTextReader(New StringReader(xResponse))
        reader.SupportMultipleContent = True

        Dim serializer As New JsonSerializer()
        Dim _pnProtokolEvidencija As ngPNProtokolEvidencija = serializer.Deserialize(Of ngPNProtokolEvidencija)(reader)

        Dim _pnPNProtokolEvidencijaQry = New ngPNProtokolEvidencijaQuery()
        Dim _pnProtokolEvidencijaResult = _pnPNProtokolEvidencijaQry.FindOne(pPnId)

        If _pnProtokolEvidencijaResult Is Nothing Then
            Return NotFound()
        End If

        _pnProtokolEvidencijaResult.PnProtokol = If(_pnProtokolEvidencija.PnProtokol Is Nothing, _pnProtokolEvidencijaResult.PnProtokol, _pnProtokolEvidencija.PnProtokol)
        _pnProtokolEvidencijaResult.PnRedBr = If(_pnProtokolEvidencija.PnRedBr = 0, _pnProtokolEvidencijaResult.PnRedBr, _pnProtokolEvidencija.PnRedBr)
        _pnProtokolEvidencijaResult.PnMjesec = If(_pnProtokolEvidencija.PnMjesec = 0, _pnProtokolEvidencijaResult.PnMjesec, _pnProtokolEvidencija.PnMjesec)
        _pnProtokolEvidencijaResult.PnGodina = If(_pnProtokolEvidencija.PnGodina = 0, _pnProtokolEvidencijaResult.PnGodina, _pnProtokolEvidencija.PnGodina)
        _pnProtokolEvidencijaResult.EmployeeId = If(_pnProtokolEvidencija.EmployeeId = 0, _pnProtokolEvidencijaResult.EmployeeId, _pnProtokolEvidencija.EmployeeId)


        _pnProtokolEvidencijaResult.Update()

        Return Json(_pnProtokolEvidencijaResult)

    End Function

    <HttpDelete>
    <Authorize>
    Public Function pn_protokolevid(ByVal pPnId As String) As IHttpActionResult

        Dim _pnId As Integer = pPnId

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _protokolevidQry = New ngPNProtokolEvidencijaQuery
        Dim _protokolevidResult = _protokolevidQry.FindOne(_pnId)

        If _protokolevidResult Is Nothing Then
            Return NotFound()
        End If

        _protokolevidResult.Delete()

        Return Ok()

    End Function


    <HttpGet>
    <Authorize>
    Public Function pn_protokolevid(ByVal pPnId As Integer) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _protokolevidQry = New ngPNProtokolEvidencijaQuery
        Dim _protokolevidResult = _protokolevidQry.FindOne(pPnId)

        If _protokolevidResult Is Nothing Then
            Return NotFound()
        End If

        Return Json(_protokolevidResult)

    End Function

    <HttpGet>
    <Authorize>
    Public Function pn_protokolevid() As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _protokolevid = New ngPNProtokolEvidencijaQuery
        Dim _protokolevidLst = _protokolevid.GetAll()

        Return Json(_protokolevidLst)

    End Function


    '''''''''''''''
    ' API ProtokolPrava
    '''''''''''''''

    <HttpPost>
    <Authorize>
    Public Function pn_protokolprava(<FromBody()> ByVal entity As ngPNProtokolPrava) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        entity.CreatedBy = _ApiSession.EmployeeID
        entity.RoleBy = _ApiSession.Role

        entity.Insert()

        Return Json(entity)

    End Function

    <HttpPut>
    <Authorize>
    Public Function pn_protokolprava(ByVal pId As Integer, <FromBody()> ByVal entity As ngPNProtokolPrava) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim request = HttpContext.Current.Request

        Dim responsedata As Stream = request.InputStream
        Dim responsereader As StreamReader = New StreamReader(responsedata)
        Dim xResponse = responsereader.ReadToEnd

        Dim reader As New JsonTextReader(New StringReader(xResponse))
        reader.SupportMultipleContent = True

        Dim serializer As New JsonSerializer()
        Dim _pnProtokolPrava As ngPNProtokolPrava = serializer.Deserialize(Of ngPNProtokolPrava)(reader)

        Dim _pnPNProtokolPravaQry = New ngPNProtokolPravaQuery()
        Dim _pnProtokolPravaResult = _pnPNProtokolPravaQry.FindOne(pId)

        If _pnProtokolPravaResult Is Nothing Then
            Return NotFound()
        End If

        _pnProtokolPravaResult.ProtokolId = If(_pnProtokolPrava.ProtokolId = 0, _pnProtokolPravaResult.ProtokolId, _pnProtokolPrava.ProtokolId)
        _pnProtokolPravaResult.EmployeeId = If(_pnProtokolPrava.EmployeeId = 0, _pnProtokolPravaResult.EmployeeId, _pnProtokolPrava.EmployeeId)
        _pnProtokolPravaResult.CreatedBy = If(_pnProtokolPrava.CreatedBy = 0, _pnProtokolPravaResult.CreatedBy, _pnProtokolPrava.CreatedBy)
        _pnProtokolPravaResult.RoleBy = If(_pnProtokolPrava.RoleBy Is Nothing, _pnProtokolPravaResult.RoleBy, _pnProtokolPrava.RoleBy)



        _pnProtokolPravaResult.Update()

        Return Json(_pnProtokolPravaResult)

    End Function

    <HttpDelete>
    <Authorize>
    Public Function pn_protokolprava(ByVal pId As String) As IHttpActionResult

        Dim _pnId As Integer = pId

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _protokolpravaQry = New ngPNProtokolPravaQuery
        Dim _protokolpravaResult = _protokolpravaQry.FindOne(_pnId)

        If _protokolpravaResult Is Nothing Then
            Return NotFound()
        End If

        _protokolpravaResult.Delete()

        Return Ok()

    End Function


    <HttpGet>
    <Authorize>
    Public Function pn_protokolprava(ByVal pId As Integer) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _protokolpravaQry = New ngPNProtokolPravaQuery
        Dim _protokolpravaResult = _protokolpravaQry.FindOne(pId)

        If _protokolpravaResult Is Nothing Then
            Return NotFound()
        End If

        Return Json(_protokolpravaResult)

    End Function

    <HttpGet>
    <Authorize>
    Public Function pn_protokolprava() As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _protokolprava = New ngPNProtokolPravaQuery
        Dim _protokolpravaLst = _protokolprava.GetAll()

        Return Json(_protokolpravaLst)

    End Function



    '''''''''''''''
    ' API Protokol
    '''''''''''''''

    <HttpPost>
    <Authorize>
    Public Function protokol(<FromBody()> ByVal entity As ngProtokol) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        entity.Insert()

        Return Json(entity)

    End Function

    <HttpPut>
    <Authorize>
    Public Function protokol(ByVal pId As Integer, <FromBody()> ByVal entity As ngProtokol) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim request = HttpContext.Current.Request

        Dim responsedata As Stream = request.InputStream
        Dim responsereader As StreamReader = New StreamReader(responsedata)
        Dim xResponse = responsereader.ReadToEnd

        Dim reader As New JsonTextReader(New StringReader(xResponse))
        reader.SupportMultipleContent = True

        Dim serializer As New JsonSerializer()
        Dim _protokol As ngProtokol = serializer.Deserialize(Of ngProtokol)(reader)

        Dim _protokolQry = New ngProtokolQuery()
        Dim _protokolResult = _protokolQry.FindOne(pId)

        If _protokolResult Is Nothing Then
            Return NotFound()
        End If

        _protokolResult.RedBr = If(_protokol.RedBr = 0, _protokolResult.RedBr, _protokol.RedBr)
        _protokolResult.Protokol = If(_protokol.Protokol Is Nothing, _protokolResult.Protokol, _protokol.Protokol)
        _protokolResult.Godina = If(_protokol.Godina = 0, _protokolResult.Godina, _protokol.Godina)
        _protokolResult.OrgJed = If(_protokol.OrgJed Is Nothing, _protokolResult.OrgJed, _protokol.OrgJed)
        _protokolResult.Opis = If(_protokol.Opis Is Nothing, _protokolResult.Opis, _protokol.Opis)



        _protokolResult.Update()

        Return Json(_protokolResult)

    End Function

    <HttpDelete>
    <Authorize>
    Public Function protokol(ByVal pId As String) As IHttpActionResult

        Dim _pnId As Integer = pId

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _protokolQry = New ngProtokolQuery
        Dim _protokolResult = _protokolQry.FindOne(_pnId)

        If _protokolResult Is Nothing Then
            Return NotFound()
        End If

        _protokolResult.Delete()

        Return Ok()

    End Function


    <HttpGet>
    <Authorize>
    Public Function protokol(ByVal pId As Integer) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _protokolQry = New ngProtokolQuery
        Dim _protokolResult = _protokolQry.FindOne(pId)

        If _protokolResult Is Nothing Then
            Return NotFound()
        End If

        Return Json(_protokolResult)

    End Function

    <HttpGet>
    <Authorize>
    Public Function protokol() As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _protokolQry = New ngProtokolQuery
        Dim _protokolLst = _protokolQry.GetAll()

        Return Json(_protokolLst)

    End Function


    '''''''''''''''
    ' API Klijent Protokol
    '''''''''''''''

    <HttpPost>
    <Authorize>
    Public Function protokolevid(ByVal pId As Integer, <FromBody()> ByVal entity As ngPNProtokolEvidencija) As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)

        Dim _protokolQry = New ngProtokolQuery
        Dim _protokolResult = _protokolQry.FindOne(pId)
        If _protokolResult Is Nothing Then
            Return New Results.StatusCodeResult(HttpStatusCode.NotFound, Request)
        End If

        Dim _protokolpravaQry = New ngPNProtokolPravaQuery
        Dim _protokolpravaResult As List(Of ngPNProtokolPrava) = _protokolpravaQry.FindByProtokol(pId)

        If _protokolpravaResult Is Nothing Then
            Return New Results.StatusCodeResult(HttpStatusCode.NotFound, Request)
        End If

        Dim _forbidden As Boolean = True
        For Each el In _protokolpravaResult
            If el.EmployeeId = _ApiSession.EmployeeID Then
                _forbidden = False
                Exit For
            End If
        Next

        If _forbidden Then
            Return New Results.StatusCodeResult(HttpStatusCode.Forbidden, Request)
        End If


        entity.PnProtokol = _protokolResult.Protokol

        Dim _protokolevidQry = New ngPNProtokolEvidencijaQuery
        Dim _protokolevidResult = _protokolevidQry.GetByProtokol(entity.PnProtokol)

        If _protokolevidResult Is Nothing Then
            Return New Results.StatusCodeResult(HttpStatusCode.NotFound, Request)
        End If

        Dim _curMonth = Now().Month
        Dim _curYear = Now().Year
        Dim _lastRow = _protokolevidResult.Where(Function(s) s.PnGodina = _curYear And s.PnMjesec = _curMonth).OrderByDescending(Function(m) m.PnRedBr).FirstOrDefault

        Dim _redBr As Integer = 1
        If _lastRow IsNot Nothing Then
            _redBr = _lastRow.PnRedBr + 1
        End If


        entity.PnRedBr = _redBr
        entity.PnMjesec = _curMonth
        entity.PnGodina = _curYear
        entity.EmployeeId = _ApiSession.EmployeeID

        entity.Insert()
        NgApiPnPutniNalog.updPutNalProtokol(entity.PnId)


        Return Json(entity)

    End Function

    <HttpGet>
    <Authorize>
    Public Function protokolemp() As IHttpActionResult

        Dim _ApiSession As New NgApiSession
        _ApiSession.getClaims(Thread.CurrentPrincipal)
        '  
        Dim _log As String = _ApiSession.Email + "-" + _ApiSession.Role + ": person"
        Dim _comment As String = "Api poziv..." + ControllerContext.Request.RequestUri.ToString
        _ApiSession.insApiLog(_log, _comment)


        Dim _protokolQry = New ngProtokolQuery
        Dim _protokolLst = _protokolQry.GetByEmp(_ApiSession.EmployeeID)

        Dim _curYear = Now().Year
        Return Json(_protokolLst.Where(Function(s) s.Godina = _curYear))

    End Function


End Class
