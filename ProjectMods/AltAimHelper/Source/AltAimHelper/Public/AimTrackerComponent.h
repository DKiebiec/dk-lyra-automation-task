#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "AimTrackerComponent.generated.h"

class AActor;
class APlayerController;

UCLASS(ClassGroup=(Custom), meta=(BlueprintSpawnableComponent))
class ALTAIMHELPER_API UAimTrackerComponent : public UActorComponent
{
    GENERATED_BODY()

public:
    UAimTrackerComponent();

    virtual void TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;

    /** Aim immediately or smoothly at a world location */
    UFUNCTION(BlueprintCallable)
    void AimAtLocation(FVector TargetLocation, float InterpSpeed = 10.f, bool bSmooth = false);

    /** Start tracking an actor */
    UFUNCTION(BlueprintCallable)
    void StartTrackingActor(AActor* TargetActorInput, float InterpSpeed = 10.f, bool bSmooth = true);

    /** Stop aiming/tracking */
    UFUNCTION(BlueprintCallable)
    void StopTracking();

private:
    /** Actor currently being tracked */
    UPROPERTY()
    AActor* TrackedActor = nullptr;

    /** Target location to aim at */
    FVector CurrentTargetLocation = FVector::ZeroVector;

    /** Interpolation speed for smooth aiming */
    float RotationInterpSpeed = 10.f;

    /** Are we currently tracking/aiming? */
    bool bTracking = false;

    /** Should we smoothly interpolate rotation (true) or snap instantly (false)? */
    bool bSmoothAiming = false;

    /** Cached player controller for setting rotation */
    UPROPERTY()
    APlayerController* OwnerController = nullptr;
};