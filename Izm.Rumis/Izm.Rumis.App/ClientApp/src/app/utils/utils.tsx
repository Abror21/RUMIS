import type { User } from 'next-auth';
import { PersonProfileType } from '@/app/admin/users/components/personProfileView';
import { ListPersonProfileType, UserType } from '@/app/admin/users/components/users';
import { Workbook } from "exceljs"
import { Application } from '../types/Applications';
import { dateApplicationFormat } from './AppConfig';
import dayjs, { type Dayjs } from 'dayjs';
import * as NProgress from "nprogress";
import { AppRouterInstance } from 'next/dist/shared/lib/app-router-context.shared-runtime';
import { Resource } from '../types/Resource';
import { redirect } from 'next/navigation';
import axios from 'axios';

export type TColumn = {
  key: string,
  title: string
}

export const handleScroll = (ref: HTMLDivElement | null) => {
  if (ref) {
    window.scrollTo({
      top: ref.offsetTop - 70,
      left: 0,
      behavior: 'smooth',
    });
  }
};

export const cookieList = (token: User) => {
  if (token?.cookies) {
    return `AuthType=${token.authType}; ` + token.cookies.reduce((accumulator, currentValue) => {
      if (!currentValue.includes('AuthType')) {
        const cookieItem: string[] = currentValue.split('; ')
        const cookieNameValue: string[] = cookieItem[0].split('=')
        return accumulator + '; ' + cookieNameValue[0] + '=' + decodeURIComponent(cookieNameValue[1])
      }

      return accumulator
    }, '')
  }

  return ''
}

export const profileName = (profile: PersonProfileType | ListPersonProfileType): string => {
  if (profile.type === 'Supervisor') {
    return profile.supervisor?.name ?? ''
  }

  if (profile.type === 'EducationalInstitution') {
    return profile.educationalInstitution?.name ?? ''
  }

  return 'Valsts'
}

export const personStatus = (user: UserType): boolean => {
  if (user?.userProfiles) {
      return user.userProfiles.some(profile => { return profile.isLoggedIn === true })
  }

  return false
}

const getApplicationListValue = (value: any, item: Application, key: string) => {
  let parsedValue = value;

  switch (key) {
    case 'applicationDate':
      parsedValue = value && dayjs(value).format(dateApplicationFormat);
      break;
    case 'educationalInstitution':
    case 'supervisor':
      parsedValue = value.name;
      break;
    case 'submitterType':
    case 'resourceTargetPersonType':
    case 'resourceSubType':
    case 'applicationStatus':
      parsedValue = value.value;
      break;
    case 'resourceTargetPerson':
    case 'submitterPerson':
      const { firstName, lastName, privatePersonalIdentifier } = value.person[0];
      parsedValue = `${firstName} ${lastName} (${privatePersonalIdentifier})`;
      break;
    case 'socialStatus':
      parsedValue = value ? 'Atbilst' : 'Neatbilst';
      break;
  }

  return parsedValue;
}

const getResourceListValue = (value: any, item: Resource, key: string) => {
  let parsedValue = value;

  switch (key) {
    case 'manufacturer':
        parsedValue = value.code
        break;
    case 'manufacturerName':
        parsedValue = item.manufacturer?.value
        break;
    case 'resourceSubType':
    case 'resourceStatus':
    case 'resourceLocation':
    case 'resourceGroup':
    case 'acquisitionType':
    case 'usagePurposeType':
    case 'targetGroup':
    case 'resourceType':
        parsedValue = value?.value
        break;
    case 'manufactureYear':
        parsedValue = value ? value : null
        break;
    case 'educationalInstitution':
        parsedValue = value?.name
        break;
    case 'acquisitionsValue':
        parsedValue = value + ' eiro'
        break;
  }

  return parsedValue;
}

export const createApplicationListExcelFile = async (items: any[], columns: TColumn[]) => {
  const workbook = new Workbook()
  const alphabet = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'];

  const sheet = workbook.addWorksheet()

  columns.forEach((col, i) => {
    sheet.getCell(`${alphabet[i]}1`).value = col.title
  })

  items.forEach((item, index) => {
    columns.forEach((col, i) => {
      const parsedValue = getApplicationListValue(item[col.key], item, col.key)
      sheet.getCell(`${alphabet[i]}${index + 2}`).value = parsedValue
    })
  })

  downloadCsvFile(workbook, 'Pieteikumi')
}

export const createResourceListExcelFile = async (items: any[], columns: TColumn[]) => {
  const workbook = new Workbook()
  const alphabet = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'];

  const sheet = workbook.addWorksheet()

  columns.forEach((col, i) => {
    sheet.getCell(`${alphabet[i]}1`).value = col.title
  })

  items.forEach((item, index) => {
    columns.forEach((col, i) => {
      const parsedValue = getResourceListValue(item[col.key], item, col.key)
      sheet.getCell(`${alphabet[i]}${index + 2}`).value = parsedValue
    })
  })

  downloadCsvFile(workbook, 'Resursi')
}

const downloadCsvFile = (workbook: any, fileName: string) => {
  workbook.csv.writeBuffer().then(function (data: any) {
    var blob = new Blob([data], { type: 'text/csv' })
    var link = document.createElement('a')
    link.href = window.URL.createObjectURL(blob)
    link.download = `${fileName}.csv`
    link.click()
  })
}

export const convertDateStringToDayjs = (dateString: string): Dayjs => {
  const date = new Date(dateString);
  const offsetDate = new Date(date.getTime() + Math.abs(date.getTimezoneOffset() * 60000));

  return dayjs(offsetDate);
}

export const goToUrl = (url: string, router: AppRouterInstance) => {
  NProgress.start();
  router.push(url)
}

export const getArrayDifference = (arrayOne: any, arrayTwo: any) => {
  return arrayOne.filter(({ value: id1 }: any) => !arrayTwo.some(({ value: id2 }: any) => id2 === id1));
}

export const signOutHandler = async (token: any) => {
  try {
    const response = await axios.post(
      `${process.env.NEXT_PUBLIC_API_URL}/auth/logout`,
      {},
      {
        headers: {
          'Cookie': cookieList(token),
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token.accessToken}`,
        },
        withCredentials: true
      },
    );

    const { redirectUrl } = response.data;
    return redirectUrl;
  } catch (error) {
    console.error('Logout failed:', error);
    return '/'
  }
}
