export type ValidationType = 'ERule' | 'ECard' | 'PCard';

export interface ValidationDialogData {
    actionType:string;
    expshown: string;
    expression:string;
    parameters: any[];
    validationType: ValidationType,
    valideeId: number
}