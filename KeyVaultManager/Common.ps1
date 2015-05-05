
$KeyVaultResourceType = "Microsoft.KeyVault/vaults";
$KeyVaultApiVersion = "2014-12-19-preview";

# Internal methods --------------------
Function Get-CurrentUser{
    Param(
        [String] $SubscriptionName
    )
    # Verify that a user is logged in
    $user = Get-AzureAccount
    if($user.Length -gt 1)
    {
        # get the user with current subscription
        if($SubscriptionName)
        {
            $account = (Get-AzureSubscription -SubscriptionName $SubscriptionName).DefaultAccount
        }
        else
        {
            $account = (Get-AzureSubscription -Current).DefaultAccount
        }
        
        if(-not $account)
        {
            Throw 'There is no default user account associated with this subscription. Certificate accounts are not supported with Azure Key Vault.'
        }

        $user = Get-AzureAccount | where {$_.Id -eq $account -and $_.Type -eq 'User'}
    }
    if(-not $user)
    {
        Write-Host 'There is no user account to use for this operation please log in...' -foregroundcolor "yellow"
        $user = Add-AzureAccount
    }
    Write-Host 'The user account that is used for this operation is:' $user[0].Id -foregroundcolor "green"

    return $user
}


Function Set-DefaultSubscription{
    Param(
        [Parameter(Mandatory=$false)] 
        [String] $SubscriptionName
    )

    # Select a Subscription
    if ($SubscriptionName){
        $subscription = Select-AzureSubscription -Current -SubscriptionName $SubscriptionName
    } else {
        if(-not (Get-AzureSubscription -Current))
        {
            # Prompts for subscription
            Write-Host 'No Current subscription is available for your account. Please select one ...' -foregroundcolor Yellow
            Select-AzureSubscription
        }
    }
    $subscription = Get-AzureSubscription -Current
    Write-Host 'The subscription that is used for this operation is:' $subscription.SubscriptionName -foregroundcolor Green
}

Function Switch-ToResourceManager{
    $azureModule = Get-Module -Name Azure -ListAvailable
    if ($azureModule)
    {
        Switch-AzureMode AzureResourceManager
        Write-Verbose 'Switched to Azure Resource Manager mode'
    }
}

Function Switch-ToServiceManagement{
    $rpModule = Get-Module -Name AzureResourceManager -ListAvailable
    if ($rpModule)
    {
        Switch-AzureMode AzureServiceManagement
        Write-Verbose 'Switched to Azure Service Management mode'
    }
}

Function Setup-KeyVaultEnv{
    Param(
        [Parameter(Mandatory=$false)] 
        [String] $SubscriptionName
    )

    Azure-Version-Check
    
    # Gets the current user 
    $user = Get-CurrentUser $SubscriptionName

    # Sets the default subscription
    Set-DefaultSubscription $SubscriptionName
}

Function Azure-Version-Check{
    $expectedMinVersion = New-Object -TypeName System.Version -ArgumentList "0.8.13"

    $azureModule = Get-Module AzureResourceManager

    if (-not $azureModule)
    {
        $azureModule = Get-Module Azure
    }
            
    if ((-not $azureModule) -or ($azureModule.Version -lt $expectedMinVersion))
    {
        Throw 'Please install Azure Powershell module version 0.8.13 or newer.'
    }    
}

# Gets Object ID
Function Get-ObjectId{
    Param(
    $ObjectId,$ServicePrincipalName,$UserPrincipalName,$UserId
    )

    Write-Verbose "Getting Object ID"
    if(-not $ObjectId)
    {   
        if($ServicePrincipalName)
        {
            $ObjectId = (Get-AzureADServicePrincipal -ServicePrincipalName $ServicePrincipalName).Id
            Write-Verbose "Object ID of $ServicePrincipalName is $ObjectId"
        }
        elseif($UserPrincipalName)
        {
            $ObjectId = Get-ObjectIdByUpn $UserPrincipalName
            Write-Verbose "Object ID of $UserPrincipalName is $ObjectId"
        }
        else
        {
            Write-Host 'No object ID is selected. The current user''s object ID will be used by default' -foregroundcolor Yellow
            $ObjectId = Get-ObjectIdByUpn $UserId
            Write-Verbose "Object ID of $UserId is $ObjectId"
        }
    }
    If(-not $ObjectId)
    {
        Throw 'No object ID for the input principal was found'
    }    
    Write-Host 'The object ID that is selected is:' $ObjectId -foregroundcolor Green

    Return($ObjectId)
}

Function Get-ObjectIdByUpn{
    Param(
    $UserPrincipalName
    )

    $userByUpn = Get-AzureADUser -UserPrincipalName $UserPrincipalName
    if(-not $userByUpn)
    {
        $userByUpn = Get-AzureADUser -Mail $UserPrincipalName
    }
    return ($userByUpn).Id
}

Function Get-CurrentUserTenantId{

    return (Get-AzureSubscription -Current).TenantId
}

Function Verify-UserAccessibilityToDirectory{

    $userAccount = (Get-AzureSubscription -Current).DefaultAccount   

    $ObjectId = Get-ObjectIdByUpn $userAccount

    # Verify that the credential is not expired 
    if($error -and $error[0].ToString().Contains('Your Azure credentials have not been set up or have expired'))
    {
        $error.Clear()
        Break
    }

    $subscription = Get-AzureSubscription -Current
    if(-not $ObjectId)
    {        
        $ExceptionMessage = 
            "`nYou do not have access to the current subscription's AD users directory." + `
            "`nTherefore, your account Object ID cannot be retrieved using `n`tGet-AzureADUser -UserPrincipalName $userAccount `n`tor `n`tGet-AzureADUser -Mail $userAccount" + `
            "`nCurrent subscription name is:  " + $subscription.SubscriptionName + `
            "`nCurrent directory's tenant ID is: " + $subscription.TenantId + `
            "`nYou are seeing this error most likely because your account user type is added as ‘guest’ to the current directory." + `
            "`nPlease contact your directory administrator and make sure your account user is of type 'member' and you have enough access permission to the directory."
        
        Write-Error $ExceptionMessage
        Break
    }

    $statusMessage = "User has access to the current subscription's AD users directory" + `
            "`nCurrent subscription name is:  " + $subscription.SubscriptionName + `
            "`nCurrent directory's tenant ID is: " + $subscription.TenantId
    Write-Verbose $statusMessage
}