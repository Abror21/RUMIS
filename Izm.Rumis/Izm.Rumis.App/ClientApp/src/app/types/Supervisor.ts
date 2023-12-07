export type Supervisor = {
    id: number,
    code: string,
    name: string
}

export type SupervisorView = {
    id: string;
    code: string,
    name: string,
    status: string,
    activeResources: number,
    resourcesInUsePersonally: number,
    resourcesInUseEducationally: number,
    applications: number,
    applicationsAccepted: number,
    applicationsAwaitingResources: number,
    applicationsPostponed: number,
    educationalInstitutions: number,
    activeEducationalInstitutions: number,
    countryDocumentTemplates: number,
    educationalInstitutionsDocumentTemplates: number,
    educationalInstitutionsDocumentLinks: number
}

export type SupervisorList = {
    id: number,
    code: string,
    name: string,
    status: boolean | null,
    educationalInstitutions: number,
    activeEducationalInstitutions: number
}

export type SupervisorListFilter = {
    sort?: string;
    sortDir?: number;
    page: number;
    take: number;
    supervisorIds?: string[],
    supervisorIsActive?: boolean | null
}