export interface FormRequestDto {
    name?: string;
    allowedOrigins?: string[];
    fields?: FormField[];
    botValidationProvider?: FormBotValidation | null;
    subscriptionsProvider?: FormSubscriptionProviderDto | null;
}

export interface FormField {
    name: string;
    isRequired: boolean;
    type?: string; // 'text', 'int', 'double', 'date', 'time', 'dateTime', 'email', 'boolean'
    // ... other specific fields merged for simplicity or use union types if needed
    // For now, using a loose interface to cover all bases
    min?: string | number;
    max?: string | number;
    minLen?: number;
    maxLen?: number;
    regex?: string;
    allowedValues?: string[];
}

export interface FormBotValidation {
    provider: string;
    parameters?: { [key: string]: string };
}

export interface FormSubscriptionProviderDto {
    provider?: string | null;
    fieldReferences?: FormSubscriptionProviderFieldsDto | null;
}

export interface FormSubscriptionProviderFieldsDto {
    email?: string | null;
    name?: string | null;
    lastname?: string | null;
    phone?: string | null;
}

export interface EmailTemplateRequestDto {
    subjectTemplate?: string;
    bodyTemplate?: string;
    fromName?: string | null;
    to: string[];
    cc?: string[] | null;
    bcc?: string[] | null;
    replyTo?: string[] | null;
    isEnabled: boolean;
}

export interface BotValidationProviderDto {
    name: string;
    parameters: { [key: string]: BotValidationParameterInfo };
}

export interface BotValidationParameterInfo {
    description: string;
    isRequired: boolean;
}

export interface ErrorDto {
    code: string;
    message: string;
    cause?: string | null;
    errors?: { [key: string]: string[] } | null;
    traceId?: string | null;
}
