# Infrastructure

This directory contains the infrastructure-related configurations and scripts essential for setting up and managing the project's cloud environments. Below is an overview of the key subdirectories:

## infra/aws/

Contains PowerShell scripts and configuration files for setting up AWS resources. Key components include:

- **aws-setup.ps1**: Automates the creation of IAM users, policies, and secrets in AWS Secrets Manager based on the `configuration.yaml` file.
- **configuration.yaml**: YAML configuration file specifying IAM usernames, policies, and secrets details.
- **readme.md**: Documentation detailing the AWS setup process, configuration parameters, and usage instructions.
- **values-tilt.yaml**: Configuration values for testing the AWS-related components with Tilt.

## infra/azure/

Houses PowerShell scripts for deploying and managing Azure resources. Key components include:

- **Deploy-OfficeConnector.ps1**: Automates the creation of Azure Entra ID applications required for syncing Office 365 data with Network Perspective.
- **Deploy-OfficeConnectorKeyVault.ps1**: Creates Azure Entra ID Applications and stores client IDs and secrets in an Azure Key Vault.
- **Set-PermissionsForOfficeConnector.ps1**: Updates Azure Entra ID Applications to request necessary permissions.
- **readme.md**: Documentation outlining the Azure deployment scripts, their purposes, prerequisites, usage instructions, and manual steps required post-deployment.
- **Set-PermissionsForOfficeConnector.ps1**: Script to update application permissions in Azure Entra ID.

## infra/helm-charts/

Contains Helm charts for deploying Kubernetes applications. Key components include:

- **np-worker/**: Helm chart for deploying the `np-worker` application, including templates for Kubernetes resources like deployments, services, and network policies.
  - **Chart.yaml**: Defines the Helm chart metadata.
  - **templates/**:
    - **deployment.yaml**: Kubernetes deployment configuration for the `np-worker`.
    - **serviceaccount.yaml**: Service account configuration for Kubernetes.
    - **networkpolicy.yaml**: Network policies for the Kubernetes deployment.
    - **_helpers.tpl**: Template helpers for the Helm chart.
    - **NOTES.txt**: Post-installation notes and instructions.

## infra/k8s/

Includes scripts and documentation for managing Kubernetes deployments. Key components include:

- **install-chart.ps1**: PowerShell script to install or upgrade the `np-worker` Helm chart using predefined values.
- **test-chart.ps1**: Script used to lint and template the `np-worker` Helm chart, aiding in validation and preview.
- **test-install-chart.ps1**: Script to install the `np-worker` Helm chart from a test registry with specific image repository settings.
- **readme.md**: Documentation detailing the Kubernetes installation and testing scripts, including usage examples and troubleshooting tips.

