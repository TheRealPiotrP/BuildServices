#.ExternalHelp KeyVaultManager.psm1-help.xml

Function New-AzureKeyVault {
    Param(
        [Parameter(Mandatory=$true,Position=0,ValueFromPipelineByPropertyName=$false,HelpMessage="Enter a unique vault name for your Azure key vault")]
        [ValidateNotNullOrEmpty()]
        [ValidatePattern('^[a-zA-Z0-9-]{3,24}$')]
        [String] $VaultName
    , 
        [Parameter(Mandatory=$true,Position=1,ValueFromPipelineByPropertyName=$true,HelpMessage="Enter a new or an existing resource group name for your Azure key vault")] 
        [ValidateNotNullOrEmpty()]
        [String] $ResourceGroupName
    ,
        [Parameter(Mandatory=$true,Position=2,ValueFromPipelineByPropertyName=$true,HelpMessage="Enter a valid location where the Azure key vault will be created")]
        [ValidateNotNullOrEmpty()]
        [String] $Location
    ,
        [Parameter(Mandatory=$false,Position=3,ValueFromPipelineByPropertyName=$true,HelpMessage="Enter true if the vault is to be enabled for deployment after creation and false otherwise. If not provided, it is set to false by default.")]
        [switch] $EnabledForDeployment
    ,
        [Parameter(Mandatory=$false,Position=4,ValueFromPipelineByPropertyName=$true,HelpMessage="Enter the subscription name you want to use for this process or your account default subscription will be used")]
        [String] $SubscriptionName
    ,
        [Parameter(Mandatory=$false,Position=5,ValueFromPipelineByPropertyName=$true,HelpMessage="Enter a valid Sku. By setting SKU to 'premium' HSM keys are allowed")] 
        [ValidateSet("standard", "premium")]
        [String] $Sku = 'standard'
    )

    Setup-KeyVaultEnv $SubscriptionName

    # Swith to Resource Manager
    Switch-ToResourceManager

    $TenantId = Get-CurrentUserTenantId

    Verify-UserAccessibilityToDirectory
    $userAccount = (Get-AzureSubscription -Current).DefaultAccount 

    # Make sure the selected resource group name is available, if not create it
    try
    {
        $resourceGroup = Get-AzureResourceGroup -Name $ResourceGroupName -ErrorAction Stop
    }
    catch
    {
        Write-Verbose "The selected resource group name $ResourceGroupName is not available."
        Write-Verbose "Creating the Resource Group $ResourceGroupName"
        $resourceGroup = New-AzureResourceGroup -ResourceGroupName $ResourceGroupName -Location $Location
    }
    Write-Verbose "Resource Group $ResourceGroupName is created/selected"

    $vaultId = @{
        "ResourceType" = $KeyVaultResourceType;
        "ApiVersion" = $KeyVaultApiVersion;
        "ResourceGroupName" = $resourceGroupName;
        "Name" = $vaultName;
    }

    $vaultProperties = @{
        "enabledForDeployment" = $EnabledForDeployment.IsPresent;
        "tenantId" = $TenantId;

        "sku" = @{
            "family" = "A";
            "name" = $Sku;
        }
        "accessPolicies" = @();
    }

    $keyVault = New-AzureResource @vaultId -PropertyObject $vaultProperties -Location $location

    if($keyVault)
    {
        Write-Verbose "Key Vault $VaultName is created at $Location"
    }
    else
    {
        Throw "Key Vault $VaultName was not created successfully."
    }

    $ObjectId = Get-ObjectIdByUpn $userAccount
    $keyVault = Set-AzureKeyVaultAccessPolicy -VaultName $VaultName `
                                  -PermissionsToKeys get,create,delete,list,update,import,backup,restore `
                                  -PermissionsToSecrets all `
                                  -ObjectId $ObjectId
          
    Write-Verbose "Vault level permissions to keys and secrets are granted to $userAccount with Object ID: $ObjectId and Tenant ID: $TenantId"

    If($Sku -eq 'standard' -and $keyVault)
    {
        Write-Host "The key vault you have created does not support HSM-protected keys. Please refer to http://go.microsoft.com/fwlink/?linkid=512521 for the key vault service tiers." `
                   "`nWhen you create a key vault using New-AzureKeyVault, specify the SKU parameter to select a service tier that supports HSM-protected keys." -ForegroundColor Yellow
    }

    Return($keyVault)
}
