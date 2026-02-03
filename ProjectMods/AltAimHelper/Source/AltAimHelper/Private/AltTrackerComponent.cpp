#include "AimTrackerComponent.h"
#include "GameFramework/Pawn.h"
#include "GameFramework/PlayerController.h"
#include "Camera/CameraComponent.h"
#include "Kismet/KismetMathLibrary.h"

UAimTrackerComponent::UAimTrackerComponent()
{
    PrimaryComponentTick.bCanEverTick = true;
    bAutoActivate = true; // ensures TickComponent runs automatically
}

void UAimTrackerComponent::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
    Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

    if (!bTracking || !OwnerController || !GetOwner())
        return;

    // Update current target location smoothly if bSmoothAiming
    if (bSmoothAiming && TrackedActor && IsValid(TrackedActor))
    {
        CurrentTargetLocation = FMath::VInterpTo(CurrentTargetLocation, TrackedActor->GetActorLocation(), DeltaTime, RotationInterpSpeed);
    }

    // Determine rotation from pawn location (or camera if attached)
    FVector StartLocation = GetOwner()->GetActorLocation();
    UCameraComponent* CameraComp = GetOwner()->FindComponentByClass<UCameraComponent>();
    if (CameraComp)
    {
        StartLocation = CameraComp->GetComponentLocation();
    }

    FRotator TargetRot = UKismetMathLibrary::FindLookAtRotation(StartLocation, CurrentTargetLocation);

    // Interpolate rotation if smooth aiming
    FRotator CurrentRot = OwnerController->GetControlRotation();
    FRotator NewRot = bSmoothAiming
        ? FMath::RInterpTo(CurrentRot, TargetRot, DeltaTime, RotationInterpSpeed)
        : TargetRot; // Snap immediately for one-shot aiming

    OwnerController->SetControlRotation(NewRot);
}

void UAimTrackerComponent::StartTrackingActor(AActor* TargetActorInput, float InterpSpeed, bool bSmooth)
{
    if (!OwnerController)
        OwnerController = Cast<APlayerController>(GetOwner()->GetInstigatorController());

    if (!TargetActorInput || !OwnerController)
        return;

    TrackedActor = TargetActorInput;
    RotationInterpSpeed = InterpSpeed;
    bTracking = true;
    bSmoothAiming = bSmooth;

    CurrentTargetLocation = TrackedActor->GetActorLocation();
}

void UAimTrackerComponent::AimAtLocation(FVector TargetLocation, float InterpSpeed, bool bSmooth)
{
    if (!OwnerController)
        OwnerController = Cast<APlayerController>(GetOwner()->GetInstigatorController());

    if (!OwnerController)
        return;

    RotationInterpSpeed = InterpSpeed;
    bTracking = true;
    bSmoothAiming = bSmooth;
    CurrentTargetLocation = TargetLocation;

    if (!bSmoothAiming)
    {
        // Snap immediately for one-shot aim
        FRotator TargetRot = UKismetMathLibrary::FindLookAtRotation(GetOwner()->GetActorLocation(), CurrentTargetLocation);
        OwnerController->SetControlRotation(TargetRot);
    }
}

void UAimTrackerComponent::StopTracking()
{
    bTracking = false;
    TrackedActor = nullptr;
}