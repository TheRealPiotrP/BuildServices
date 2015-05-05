#.ExternalHelp KeyVaultManager.psm1-help.xml

Function Get-AzureKeyVault{    
    [CmdLetBinding(DefaultParameterSetName="None")]
    Param(
        [Parameter(Mandatory=$true,Position=0,ParameterSetName='ByVaultName',ValueFromPipelineByPropertyName=$true,HelpMessage="Enter an Azure key vault name to retrieve")]
        [ValidateNotNullOrEmpty()]
        [String] $VaultName
    , 
        [Parameter(Mandatory=$true,Position=0,ParameterSetName='ByResourceGroupName',ValueFromPipelineByPropertyName=$true,HelpMessage="Enter a resource group name for your Azure key vault")] 
        [ValidateNotNullOrEmpty()]
        [String] $ResourceGroupName
    ,
        [Parameter(Mandatory=$false,Position=1,ValueFromPipelineByPropertyName=$true,HelpMessage="Enter the subscription name you want to use for this process or your account's default subscription will be used")]
        [String] $SubscriptionName
    )
    
    Setup-KeyVaultEnv $SubscriptionName

    # Swith to Resource Manager
    Switch-ToResourceManager
    
    $vaults = Get-AzureResource -ResourceType $KeyVaultResourceType

    if($VaultName)
    {
        $vault = $vaults | where {$_.Name -eq $VaultName}

        if($vault)
        {
            $vaultId = @{
                "ResourceType" = $KeyVaultResourceType;
                "ApiVersion" = $KeyVaultApiVersion;
                "ResourceGroupName" = $vault.ResourceGroupName;
                "Name" = $vaultName;
            }
            $vaults = Get-AzureResource @vaultId
        }
        else
        {
            $vaults = $null
        }
    }
    elseif($ResourceGroupName -and $vaults)
    {
        $vaults = $vaults | where {$_.ResourceGroupName -eq $ResourceGroupName}
    }

    if(-not $vaults)
    {
        Write-Host 'No Azure key vault is found!' -ForegroundColor Yellow
    }

    Return($vaults)
}