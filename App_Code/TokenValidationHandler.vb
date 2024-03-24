Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.IdentityModel.Tokens.Jwt
Imports System.Linq
Imports System.Net
Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Web
Imports Microsoft.IdentityModel.Tokens
Imports System.Runtime.InteropServices
Imports System.Security.Principal

Public Class TokenValidationHandler
    Inherits DelegatingHandler

    Private Shared Function TryRetrieveToken(ByVal request As HttpRequestMessage, <Out> ByRef token As String) As Boolean
        token = Nothing
        Dim authzHeaders As IEnumerable(Of String)

        If Not request.Headers.TryGetValues("Authorization", authzHeaders) OrElse authzHeaders.Count() > 1 Then
            Return False
        End If

        Dim bearerToken = authzHeaders.ElementAt(0)
        token = bearerToken.Replace("Bearer", "")
        token = token.TrimStart(" ")
        token = token.TrimStart("<")
        token = token.TrimEnd(">")

        Return True
    End Function

    Protected Overrides Function SendAsync(ByVal request As HttpRequestMessage, ByVal cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)
        Dim statusCode As HttpStatusCode
        Dim token As String = ""

        If Not TryRetrieveToken(request, token) Then
            statusCode = HttpStatusCode.Unauthorized
            Return MyBase.SendAsync(request, cancellationToken)
        End If



        Try
            Const secretKey As String = "your secret key goes here"
            Dim securityKey = New SymmetricSecurityKey(System.Text.Encoding.[Default].GetBytes(secretKey))
            Dim tokenHandler = New JwtSecurityTokenHandler()
            Dim validationParameters = New TokenValidationParameters With {
                .ValidAudience = "http://zov-consulting.ba/:8080/",
                .ValidIssuer = "http://zov-consulting.ba/:8080/",
                .ValidateLifetime = True,
                .ValidateIssuerSigningKey = True,
                .LifetimeValidator = AddressOf LifetimeValidator,
                .IssuerSigningKey = securityKey
            }
            Dim validatedToken As SecurityToken = Nothing
            Thread.CurrentPrincipal = tokenHandler.ValidateToken(token, validationParameters, validatedToken)
            HttpContext.Current.User = tokenHandler.ValidateToken(token, validationParameters, validatedToken)


            Return MyBase.SendAsync(request, cancellationToken)
        Catch __unusedSecurityTokenValidationException1__ As SecurityTokenValidationException
            statusCode = HttpStatusCode.Unauthorized
        Catch __unusedException2__ As Exception
            statusCode = HttpStatusCode.InternalServerError
        End Try


        Return Task(Of HttpResponseMessage).Factory.StartNew(Function() New HttpResponseMessage(statusCode), cancellationToken)
    End Function

    Public Function LifetimeValidator(ByVal notBefore As DateTime?, ByVal expires As DateTime?, ByVal securityToken As SecurityToken, ByVal validationParameters As TokenValidationParameters) As Boolean
        If expires Is Nothing Then Return False
        Return DateTime.UtcNow < expires
    End Function
End Class
