! MODEL_2 -- NEXT STEP ONLY: make the seven compliant bottom seams firmer.
! Keeps the correct bottom faces: PART4,6,8,10,12,14,16,18.
! Keeps the existing single motion FOLD_SERVO_BASE_2_4 unchanged.
! Creates no joints and no extra motions.
!
! The first two stiffness values are the hinge's transverse rotations;
! the third one remains very small so rotation about local Z can still fold.

force modify element_like bushing &
    bushing_name = .MODEL_2.BUSH_BASE_2_6 &
    stiffness = 20, 20, 20 &
    damping = 0.20, 0.20, 0.20 &
    tstiffness = 20, 20, 0.001 &
    tdamping = 0.20, 0.20, 0.002

force modify element_like bushing &
    bushing_name = .MODEL_2.BUSH_BASE_2_8 &
    stiffness = 20, 20, 20 &
    damping = 0.20, 0.20, 0.20 &
    tstiffness = 20, 20, 0.001 &
    tdamping = 0.20, 0.20, 0.002

force modify element_like bushing &
    bushing_name = .MODEL_2.BUSH_BASE_2_10 &
    stiffness = 20, 20, 20 &
    damping = 0.20, 0.20, 0.20 &
    tstiffness = 20, 20, 0.001 &
    tdamping = 0.20, 0.20, 0.002

force modify element_like bushing &
    bushing_name = .MODEL_2.BUSH_BASE_2_12 &
    stiffness = 20, 20, 20 &
    damping = 0.20, 0.20, 0.20 &
    tstiffness = 20, 20, 0.001 &
    tdamping = 0.20, 0.20, 0.002

force modify element_like bushing &
    bushing_name = .MODEL_2.BUSH_BASE_2_14 &
    stiffness = 20, 20, 20 &
    damping = 0.20, 0.20, 0.20 &
    tstiffness = 20, 20, 0.001 &
    tdamping = 0.20, 0.20, 0.002

force modify element_like bushing &
    bushing_name = .MODEL_2.BUSH_BASE_2_16 &
    stiffness = 20, 20, 20 &
    damping = 0.20, 0.20, 0.20 &
    tstiffness = 20, 20, 0.001 &
    tdamping = 0.20, 0.20, 0.002

force modify element_like bushing &
    bushing_name = .MODEL_2.BUSH_BASE_2_18 &
    stiffness = 20, 20, 20 &
    damping = 0.20, 0.20, 0.20 &
    tstiffness = 20, 20, 0.001 &
    tdamping = 0.20, 0.20, 0.002
