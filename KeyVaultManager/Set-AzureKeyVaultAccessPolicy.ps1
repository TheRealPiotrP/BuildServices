#.ExternalHelp KeyVaultManager.psm1-help.xml

function Set-AzureKeyVaultAccessPolicy {
    [CmdLetBinding(DefaultParameterSetName="None")]
    param(
        [Parameter(Mandatory=$true,Position=0,ValueFromPipelineByPropertyName=$true,HelpMessage="Enter the vault name that you want to update")] 
        [ValidateNotNullOrEmpty()]
        [String] $VaultName
    , 
        [Parameter(Mandatory=$true,Position=1,ParameterSetName='ByObjectId',ValueFromPipelineByPropertyName=$true,HelpMessage="Enter a valid object id or your account's object id will be used by default")]
        [System.Guid] $ObjectId
    ,
        [Parameter(Mandatory=$true,Position=1,ParameterSetName='ByServicePrincipalName',ValueFromPipelineByPropertyName=$true,HelpMessage="Enter a valid service principal name")]
        [ValidateNotNullOrEmpty()]
        [string] $ServicePrincipalName
    ,
        [Parameter(Mandatory=$true,Position=1,ParameterSetName='ByUserPrincipalName',ValueFromPipelineByPropertyName=$true,HelpMessage="Enter a valid user principal name")]
        [ValidateNotNullOrEmpty()]
        [string] $UserPrincipalName
    ,
        [Parameter(Mandatory=$false,Position=2,ValueFromPipelineByPropertyName=$true,HelpMessage="Enter the list of key permissions that you want to grant to the new user/service principal")] 
        [ValidateSet("decrypt", "encrypt", "unwrapKey", "wrapKey", "verify", "sign", "get", "list", "update", "create", "import", "delete", "backup", "restore", "all")]
        [string[]] $PermissionsToKeys
    , 
        [Parameter(Mandatory=$false,Position=3,ValueFromPipelineByPropertyName=$true,HelpMessage="Enter the list of secret permissions that you want to grant to the new user/service principal")] 
        [ValidateSet("get", "list", "set", "delete", "all")]
        [string[]] $PermissionsToSecrets
    ,
        [Parameter(Mandatory=$false,Position=4,ValueFromPipelineByPropertyName=$true,HelpMessage="Enter the subscription name you want to use for this process or your account default subscription will be used")] 
        [String] $SubscriptionName
    )

    if(-not ($PermissionsToSecrets -or $PermissionsToKeys))
    {
        throw "At least one permission should be selected for key access or secret access!"
    }

    $vault = Get-AzureKeyVault -VaultName $VaultName -SubscriptionName $SubscriptionName
    if(-not $vault)
    {
        throw 'ResourceNotFound: The vault ' + $VaultName + ' is not available'
    }

    $TenantId = $vault.Properties["tenantId"]
    
    # get the user with current subscription
    $user = Get-CurrentUser -SubscriptionName $SubscriptionName    

    $ObjectId = Get-ObjectId -ObjectId $ObjectId `
                             -ServicePrincipalName $ServicePrincipalName `
                             -UserPrincipalName $UserPrincipalName `
                             -UserId $user.Id
 
    $permissions = @{}
    if($PermissionsToKeys)
    {
        $permissions.Add("keys", $PermissionsToKeys);  
    }
    if($PermissionsToSecrets)
    {
        $permissions.Add("secrets", $PermissionsToSecrets);
    }

    $policyEntry = @{ 
        "tenantId" = $TenantId; 
        "objectId" = $ObjectId; 
        "permissions" = $permissions;
    } 

    $n = $vault.Properties.accessPolicies.RemoveAll({ param($p) $p.tenantId -eq $TenantId -and $p.objectId -eq $ObjectId})
    $vault.Properties.accessPolicies.Add($policyEntry)
    
    Return (Set-AzureResource -ApiVersion $KeyVaultApiVersion `
                              -Name $VaultName `
                              -ResourceGroupName $vault.ResourceGroupName `
                              -ResourceType $KeyVaultResourceType `
                              -PropertyObject $vault.Properties)
}
