Imports System.IdentityModel.Tokens.Jwt
Imports System.Net
Imports System.Security.Claims
Imports System.Web.Http
Imports System.Web.Http.Cors
Imports Microsoft.IdentityModel.Tokens
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Serialization
Imports System.Web.SessionState
Imports MySql.Data.MySqlClient
Imports MySql.Data
Imports System.Data
Imports System.Reflection
Imports System.Threading
Imports System.IdentityModel

'<EnableCors("*", "*", "*")>
Public Class JwtController
    Inherits ApiController

    <HttpOptions>
    <Authorize>
    <Route("ok")>
    Public Function fdfdf() As IHttpActionResult
        Return Ok("Authenticated")
    End Function

    <HttpGet>
    <Authorize>
    <Route("ok")>
    Public Function Authenticated() As IHttpActionResult
        Return Ok("Authenticated")
    End Function

    <HttpGet>
    <Route("notok")>
    Public Function NotAuthenticated() As IHttpActionResult
        Return Unauthorized()
    End Function

    '<EnableCors("http://localhost:56300", "*", "*")>
    <HttpPost>
    <Route("jwt")>
    Public Function Login(
        <FromBody> ByVal loginVM As NgLogin) As IHttpActionResult
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

        If isUsernamePasswordValid Then
            Dim token = _ApiSession.CreateToken()

            Dim _JwtOK As New NgJWTOk
            _JwtOK.success = True
            _JwtOK.token = token
            Dim jsonLwtOk As String = JsonConvert.SerializeObject(_JwtOK, Formatting.None)

            'jsonLwtOk = "{ 'success': true,  'token': 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJkYXRhIjoiYWRtaW4iLCJleHBpcmVzSW4iOiIxaCIsImlhdCI6MTU0NDMxMDE5OX0.A4xmHtGl28lUeaSqLJRqwkcn_w7MQYyFEuHNq2trI8I' }"

            Return Json(_JwtOK)
        End If

        loginResponse.responseMsg.StatusCode = HttpStatusCode.Unauthorized
        Dim response As IHttpActionResult = ResponseMessage(loginResponse.responseMsg)
        Return response
    End Function

    <HttpGet>
    Public Function Products() As IHttpActionResult

        Dim _Json As String = <![CDATA[ 
          [
            { id: 1, name: "Kayak", category: "Watersports",
                description: "A boat for one person", price: 275 },
            { id: 2, name: "Lifejacket", category: "Watersports",
                description: "Protective and fashionable", price: 48.95 },
            { id: 3, name: "Soccer Ball", category: "Soccer",
                description: "FIFA-approved size and weight", price: 19.50 },
            { id: 4, name: "Corner Flags", category: "Soccer",
                description: "Give your playing field a professional touch",
                price: 34.95 },
            { id: 5, name: "Stadium", category: "Soccer",
                description: "Flat-packed 35,000-seat stadium", price: 79500 },
            { id: 6, name: "Thinking Cap", category: "Chess",
                description: "Improve brain efficiency by 75%", price: 16 },
            { id: 7, name: "Unsteady Chair", category: "Chess",
                description: "Secretly give your opponent a disadvantage",
                price: 29.95 },
            { id: 8, name: "Human Chess Board", category: "Chess",
                description: "A fun game for the family", price: 75 },
            { id: 9, name: "Bling Bling King", category: "Chess",
                description: "Gold-plated, diamond-studded King", price: 1200 }
                ]
]]>.Value

        Dim unserializedContent = JsonConvert.DeserializeObject(Of List(Of NgProduct))(_Json)

        Return Json(unserializedContent)

    End Function

    <HttpGet>
    <Authorize>
    Public Function Orders() As IHttpActionResult

        Dim _Json As String = <![CDATA[ 
          [
            { id: 1, name: "Kupac 1", address: "Brcanska 12",
                city: "Sarajevo", state: "Sarajevo", zip:"71000", country:"Bosna", shipped:false },
            { id: 2, name: "Kupac 2", address: "Brcanska 22",
                city: "Sarajevo", state: "Sarajevo", zip:"71000", country:"Bosna", shipped:false }
           ]     
]]>.Value

        Dim unserializedContent = JsonConvert.DeserializeObject(Of List(Of NgOrder))(_Json)

        Return Json(unserializedContent)

    End Function


End Class

