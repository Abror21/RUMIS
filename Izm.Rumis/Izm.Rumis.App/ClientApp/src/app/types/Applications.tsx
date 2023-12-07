import { Person } from "./Persons";

export type Application =  {
    applicationNumber: string;
    applicationDate: string;
    applicationResources: {
      id: string;
      pnaNumber: string,
      pnaStatus: {
        id: string;
        code: string;
        value: string;
      }
    }[] | null
    founderId: string;
    educationalInstitution: {
        name: string;
        code: string;
        id: string; 
    };
    submitterPerson: {
      person: Person[];
    }
    submitterType: {
        value: string;
        code: string;
        id: string; 
    };
    resourceTargetPerson: {
      id: string;
      person: Person[];
      persons: Person[];
    };
    resourceTargetPersonType: {
      value: string;
      code: string;
      id: string; 
    };
    socialStatus: boolean;
    socialStatusApproved?: boolean;
    show?: boolean;
    applicationStatus: {
      value: string;
      code: string;
      id: string; 
    };
    resourceType: {
      code: string;
      id: string;
      value: string
    }
    resourceSubType: {
      code: string;
      id: string;
      value: string
    }
    id: string;
    resourceTargetPersonClassGrade?: string;
    resourceTargetPersonClassParallel?: string;
    contactPerson?: {
      contacts: {
        id: string;
        contactValue: string;
        contactType: {
          code: string;
          id: string;
          value: string
        }
      }[],
      contactData: {
        value: string;
        type: {id: string; code: string; value: string}
      }[];
        id: string;
        person: Person[];
    },
    resourceTargetPersonWorkStatus?: {
      code: string;
      id: string;
      value: string
    }
    resourceTargetPersonEducationalStatus?: {
      code: string;
      id: string;
      value: string
    }
    resourceTargetPersonEducationalSubStatus?: {
      code: string;
      id: string;
      value: string
    }
    resourceTargetPersonEducationalProgram?: string
    resourceTargetPersonGroup?: string
    applicationSocialStatus: {
      id: string
      socialStatusApproved: boolean | null
      socialStatus: {
        code: string;
        id: string;
        value: string
      }
    }[]
    supervisor: {
      code: string;
      id: string;
      name: string
    }
  }

export type CheckDuplicateResponse = {
  educationalInstitution: {
    code: string;
    id: number;
    name: string;
  };
}