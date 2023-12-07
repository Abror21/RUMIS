'use client';

import {
  Dropdown,
  Table,
  Button,
} from 'antd';
import type { ColumnsType, TablePaginationConfig } from 'antd/es/table';
import {useEffect, useRef, useState} from 'react';
import { AppConfig } from '@/app/utils/AppConfig';
import useQueryApiClient from '@/app/utils/useQueryApiClient';
import Link from 'next/link';
import { EducationalInstitutionFilter, EducationalInstitution as EducationalInstitutionType } from '@/app/types/EducationalInstitution';
import EducationalInstitutionFilters from './EducationalInstitutionFilters';
import { EDUCATIONAL_INSTITUTION_STATUS_ACTIVE, EDUCATIONAL_INSTITUTION_STATUS_DISABLED, EDUCATIONAL_INSTITUTION_STATUS_DISCONTINUED } from '../../application/new/components/applicantConstants';
import { useSession } from 'next-auth/react';
import { ButtonWithIcon } from '@/app/components/buttonWithIcon';
import { goToUrl } from '@/app/utils/utils';
import { useRouter, useSearchParams } from 'next/navigation';
import { SorterResult } from 'antd/es/table/interface';

export const initialValues = {
    page: 1,
    take: AppConfig.takeLimit,
};

const EducationalInstitutionList = () => {
    const searchParams = useSearchParams()

    const defaultSupervisorIds = searchParams.get('supervisorIds')
    const defaultEducationalInstitutionStatusIds = searchParams.get('educationalInstitutionStatusIds')
    
    const [filter, setFilter] = useState<EducationalInstitutionFilter>({
        ...initialValues,
        supervisorIds: defaultSupervisorIds ? [+defaultSupervisorIds] : undefined,
        educationalInstitutionStatusIds: defaultEducationalInstitutionStatusIds ? defaultEducationalInstitutionStatusIds : undefined,
    })
    
    const pageTopRef = useRef(null);
    const { data: sessionData } = useSession();
    const router = useRouter();

    const {
        data: educationalInstitutions,
        appendData: refetchWithUpdatedData,
        refetch,
        isLoading,
      } = useQueryApiClient({
        request: {
          url: '/educationalInstitutions/list',
          data: filter
        },
    });

    const {
        appendData: updateEducationalInstitution,
        isLoading: isUpdateLoading,
      } = useQueryApiClient({
        request: {
          url: '/educationalInstitutions/:id',
          method: 'PUT'
        },
        onSuccess: () => {
            refetchWithUpdatedData(filter)
        }
    });

    const changeStatus = (educationalInstitution: EducationalInstitutionType, status: string) => {
        updateEducationalInstitution({
            code: educationalInstitution.code,
            name: educationalInstitution.name,
            supervisorId: educationalInstitution.supervisor.id,
            statusId: status,
            educationalInstitutionContactPersons: educationalInstitution.educationalInstitutionContactPersons
        }, {id: educationalInstitution.id})
    }

    const columns: ColumnsType<EducationalInstitutionType> = [
        {
            title: 'Izglītības iestādes nosaukums',
            dataIndex: 'name',
            key: 'name',
            render: (name: string, record: EducationalInstitutionType) => <Link href={`/admin/educational-institution/${record.id}`}>{name}</Link>
        },
        {
            title: 'Statuss',
            dataIndex: 'status',
            key: 'status',
            render: (status: EducationalInstitutionType['status'], record: EducationalInstitutionType) => status.value
        },
        {
            title: 'Vadošā iestāde',
            dataIndex: 'supervisor',
            key: 'supervisor',
            render: (supervisor: EducationalInstitutionType['supervisor'], record: EducationalInstitutionType) => <Link href={`/admin/supervisor/${supervisor.id}`}>{supervisor.name}</Link>
        },
        {
            title: 'Darbības',
            dataIndex: 'operation',
            key: 'operation',
            width: '150px',
            render: (_: any, record: EducationalInstitutionType) => {
                switch (record.status.id) {
                    case EDUCATIONAL_INSTITUTION_STATUS_DISABLED:
                        return (
                            <Button
                                onClick={() => changeStatus(record, EDUCATIONAL_INSTITUTION_STATUS_ACTIVE)}
                            >
                                Aktivizēt
                            </Button>
                        );
                    case EDUCATIONAL_INSTITUTION_STATUS_ACTIVE:
                        return (
                            <Button
                                onClick={() => changeStatus(record, EDUCATIONAL_INSTITUTION_STATUS_DISABLED)}
                            >
                                Bloķēt
                            </Button>
                        );
                    case EDUCATIONAL_INSTITUTION_STATUS_DISCONTINUED:
                        return (
                            <Dropdown.Button
                                menu={items(record)}
                                onClick={() => changeStatus(record, EDUCATIONAL_INSTITUTION_STATUS_ACTIVE)}
                            >
                                Aktivizēt
                            </Dropdown.Button>
                        );
                }
            },
        }
    ]

    const items = (record: EducationalInstitutionType) => {
        return {
          items: [
            {
              key: '1',
              label: (
                <button type="button" onClick={() => changeStatus(record, EDUCATIONAL_INSTITUTION_STATUS_DISABLED)}>
                  Bloķēt
                </button>
              ),
            },
          ],
        };
      };

    const fetchRecords = (page: number, pageSize: number) => {
        const newFilter = { ...filter, page, take: pageSize };
        setFilter(newFilter);
        refetchWithUpdatedData(newFilter);
    };

    const handleTableChange = (
        pagination: TablePaginationConfig,
        sorter: SorterResult<EducationalInstitutionType>,
    ) => {
        const newFilter: any = {
            ...filter,
            // @ts-ignore
            sort: sorter?.field ?? undefined,
            sortDir: sorter.order
                ? (sorter.order === 'ascend') ? 0 : 1
                : undefined,
            page: pagination?.current as number,
            take: pagination?.pageSize as number
        };
        setFilter(newFilter);
        refetchWithUpdatedData(newFilter)
    };

    return (
        <div>
            <EducationalInstitutionFilters 
                activeFilters={filter}
                filterState={setFilter}
                refresh={(newFilters: EducationalInstitutionFilter) => {
                refetchWithUpdatedData(newFilters)
                // setSelectedRowKeys([])
                }}
                defaultEducationalInstitutionStatusIds={defaultEducationalInstitutionStatusIds}
                defaultSupervisorIds={defaultSupervisorIds}
            />
            <div ref={pageTopRef}>
                <div className='overflow-auto'>
                <Table
                    loading={isLoading || isUpdateLoading}
                    columns={columns}
                    dataSource={educationalInstitutions?.items}
                    pagination={{
                        current: educationalInstitutions.page,
                        total: educationalInstitutions?.total,
                        defaultPageSize: filter.take,
                        pageSizeOptions: [25, 50, 75],
                        showSizeChanger: true,
                        showTotal: (total, range) => `${range[0]}-${range[1]} no ${total} ierakstiem`,
                        onChange: (page, takeLimit) => {
                            fetchRecords(page, takeLimit);
                        },
                    }}
                    rowKey={(record) => record.id}
                />
                </div>
            </div>
        </div>
    )
}

export default EducationalInstitutionList