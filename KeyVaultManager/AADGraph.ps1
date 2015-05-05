function Get-AuthenticationResult{
    Param(
        [String]$DomainName
    )

    $clientId = "1950a258-227b-4e31-a9cf-717495945fc2"
    $redirectUri = "urn:ietf:wg:oauth:2.0:oob"
    $resourceClientId = "00000002-0000-0000-c000-000000000000"
    $resourceAppIdURI = "https://graph.windows.net"
    $authority = [string]::Format("https://login.windows.net/{0}", $DomainName)  
    $authContext = New-Object "Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext" -ArgumentList $authority,$false
    $authResult = $authContext.AcquireToken($resourceAppIdURI, $clientId, $redirectUri, "Always")
    return $authResult
}

function Connect-AzureAD{
    Param(
        [parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [String]$DomainName
    )
    PROCESS {
        $global:authenticationResult = $null
        $global:authenticationResult = Get-AuthenticationResult $DomainName
    }
}

function Get-AzureADApplication{
    Param(
        [parameter(Mandatory=$false)]
        [ValidateNotNullOrEmpty()]
        [String] $Name
    )

    $objects = $null
    if($authenticationResult -ne $null)
    {
        $header = $authenticationResult.CreateAuthorizationHeader()
        if ($Name){
            $uri = [string]::Format("https://graph.windows.net/{0}/applications?`$filter=startswith(displayName,`'{1}`')&api-version=1.5&`$top=999",$authenticationResult.TenantId, $Name)
        }
        else{
            $uri = [string]::Format("https://graph.windows.net/{0}/applications?api-version=1.5&`$top=999",$authenticationResult.TenantId)
        }
        Write-Verbose "HTTP GET $uri"
        $contentType = "application/json"
        $result = Invoke-WebRequest -Method Get -Uri $uri -ContentType $contentType -Headers @{"Authorization"=$header}
        if($result.StatusCode -eq 200)
        {
            Write-Verbose "Getting AAD applications."
            $json = (ConvertFrom-Json $result.Content)
            if($json -ne $null){$objects = $json.value}
        }
    }
    else{
        Write-Host "Not connected to an AAD tenant. First run Connect-AzureAD." -ForegroundColor Yellow
    }    
    return $objects
}

function Get-AzureADObjectById{
    Param(
        [string]$type
    , 
        [string]$id
    )
    $object = $null
    if($global:authenticationResult -ne $null){
        $header = $authenticationResult.CreateAuthorizationHeader()
        $uri = [string]::Format("https://graph.windows.net/{0}/{1}/{2}?api-version=1.5",$authenticationResult.TenantId, $type.Trim(), $id.Trim())
        Write-Verbose "HTTP GET $uri"
        $contentType = "application/json"
        $result = Invoke-WebRequest -Method Get -Uri $uri -ContentType $contentType -Headers @{"Authorization"=$header}
        if($result.StatusCode -eq 200)
        {
            Write-Verbose "Get succeeded."
            $object = (ConvertFrom-Json $result.Content)
        }
    }
    else{
        Write-Host "Not connected to an AAD tenant. First run Connect-AzureAD." -ForegroundColor Yellow
    }
    return $object
}

function Add-AzureADApplicationCredential{
    [CmdLetBinding(DefaultParameterSetName="None")]
    Param(
        [parameter(Mandatory=$true)]
        [Guid]$ObjectId
    , 
        [parameter(Mandatory=$true, ParameterSetName='Certificate')]
        [ValidateNotNullOrEmpty()]
        [string]$FilePath
    ,
        [parameter(Mandatory=$true, ParameterSetName='Password')]
        [ValidateNotNullOrEmpty()]
        [string]$Password
    )

    $newObject = $null
    if($global:authenticationResult -ne $null) 
    {
        if ($FilePath -and !(Test-Path $FilePath))
        {
            Write-Error "Given file " $FilePath " does not exist."
            return
        }

        $application = Get-AzureADObjectById "applications" $ObjectId
        $header = $authenticationResult.CreateAuthorizationHeader()
        if ($application -eq $null)
        {
            Write-Error "Could not get application for given object id '" $ObjectId "'."
            return
        }

        
        $startDate = [System.DateTime]::Now.ToString("O")
        $endDate = [System.DateTime]::Now.AddYears(1).ToString("O")
        $credentialObject = New-Object -TypeName PSObject
        $credentialObject | Add-Member -MemberType NoteProperty -Name endDate -Value $endDate -PassThru `
                          | Add-Member -MemberType NoteProperty -Name startDate -Value $startDate -PassThru

        if($FilePath)
        {
            $cer = New-Object System.Security.Cryptography.X509Certificates.X509Certificate
            $cer.Import($FilePath);
            $expiryDate = [System.DateTime]::Parse($cer.GetExpirationDateString())

            if ($expiryDate -lt [System.DateTime]::Now.AddYears(1))
            {
                Write-Error "The certificate you supplied must be valid for at least one year from now. "
                return
            }

            $binCert = $cer.GetRawCertData();
            $credValue = [System.Convert]::ToBase64String($binCert)
            $credentialObject | Add-Member -MemberType NoteProperty -Name type -Value "AsymmetricX509Cert" -PassThru `
                              | Add-Member -MemberType NoteProperty -Name usage -Value "Verify" -PassThru `
                              | Add-Member -MemberType NoteProperty -Name value -Value $credValue

            if ([bool](Get-Member -InputObject $application -Name "keyCredentials"))
            {
                [System.Collections.ArrayList]$keys = $application.keyCredentials
                $credentialCound = $keys.Add($credentialObject)
                $application.keyCredentials = $keys
            }
            else
            {
                $application | Add-Member -MemberType NoteProperty -Name keyCredentials -Value @($credentialObject)
            }
        }
        else
        {
            $credentialObject | Add-Member -MemberType NoteProperty -Name value -Value $Password
        
            if ([bool](Get-Member -InputObject $application -Name "passwordCredentials"))
            {
                [System.Collections.ArrayList]$passwords = $application.passwordCredentials

                $passwords.Add($credentialObject)
                $application.passwordCredentials = $passwords
            }
            else
            {
                $application | Add-Member -MemberType NoteProperty -Name passwordCredentials -Value @($credentialObject)
            }
        }

        $uri = [string]::Format("https://graph.windows.net/{0}/applications/{1}?api-version=1.5",$authenticationResult.TenantId, $ObjectId)
        Write-Verbose "HTTP PATCH $uri"        
        $body = ConvertTo-Json -InputObject $application
        Write-Verbose $body             
        $contentType = "application/json"
        $headers = @{"Authorization"=$header}
        $result = Invoke-WebRequest -Method Patch -Uri $uri -ContentType $contentType -Headers $headers -Body $body
        if($result.StatusCode -eq 201)
        {
          Write-Verbose "Adding new credential succeeded."
          $newObject = (ConvertFrom-Json $result.Content)
        }
    }
    else{
        Write-Host "Not connected to an AAD tenant. First run Connect-AzureAD."
    }
    return $newObject
}

function New-AzureADApplication{
    Param(
        [parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]$DisplayName
    )

    $newObject = $null
    if($global:authenticationResult -ne $null) {
        $header = $authenticationResult.CreateAuthorizationHeader()
        $uri = [string]::Format("https://graph.windows.net/{0}/applications?api-version=1.5",$authenticationResult.TenantId)
        Write-Verbose "HTTP POST $uri"        
        $appObject = New-Object -TypeName PSObject
        $identifierUri = [string]::Format("http://localhost:8080/{0}",[Guid]::NewGuid().ToString("N"))
        $appObject | Add-Member -MemberType NoteProperty -Name displayName -Value $DisplayName -PassThru `
                   | Add-Member -MemberType NoteProperty -Name identifierUris -Value @($identifierUri)
        $body = ConvertTo-Json -InputObject $appObject
        Write-Verbose $body          
        $contentType = "application/json"
        $headers = @{"Authorization"=$header}
        $result = Invoke-WebRequest -Method Post -Uri $uri -ContentType $contentType -Headers $headers -Body $body
        if($result.StatusCode -eq 201)
        {
          Write-Verbose "Application creation succeeded."
          $newObject = (ConvertFrom-Json $result.Content)
        }

        Write-Verbose "Creating corresponding Service Principal."
        $uri = [string]::Format("https://graph.windows.net/{0}/servicePrincipals?api-version=1.5",$authenticationResult.TenantId)
        $servicePrincipal = New-Object -Type PSObject
        $servicePrincipal | Add-Member -MemberType NoteProperty -Name displayName -Value $DisplayName -PassThru `
                          | Add-Member -MemberType NoteProperty -Name appId -Value $newObject.appId
        $body = ConvertTo-Json -InputObject $servicePrincipal
        Write-Verbose $body               
        $headers = @{"Authorization"=$header}
        $result = Invoke-WebRequest -Method Post -Uri $uri -ContentType $contentType -Headers $headers -Body $body
        if($result.StatusCode -eq 201)
        {
          Write-Verbose "Service Principal creation succeeded."
          $newServicePrincipal = (ConvertFrom-Json $result.Content)
        }
    }
    else{
        Write-Host "Not connected to an AAD tenant. First run Connect-AzureAD."
    }
    return $newObject
}