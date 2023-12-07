'use client';

import {
    Badge,
    Button,
    Dropdown,
    Form,
    Modal,
    Table,
} from 'antd';
import type {ColumnsType, TablePaginationConfig} from 'antd/es/table';
import {useRef, useState} from 'react';

import {AppConfig, dateApplicationFormat, dateFormat, pnaStatuses} from '@/app/utils/AppConfig';
import useQueryApiClient from '@/app/utils/useQueryApiClient';
import {goToUrl, handleScroll} from '@/app/utils/utils';
import dayjs from 'dayjs';
import PnActsFilters from './PnActsFilters';
import {PnAct} from '@/app/types/PnAct';
import Link from 'next/link';
import ChangeFiltersModal from './ChangeFiltersModal';
import {PnAct as DataType} from '@/app/types/PnAct';
import {useSession} from 'next-auth/react';
import {UserProfile} from '@/app/types/UserProfile';
import ChangeSequenceModal from './ChangeSequenceModal';
import {SortableTableItem} from '@/app/components/SortableTable';
import {SorterResult} from 'antd/es/table/interface';
import {useRouter} from 'next/navigation';
import {ControlOutlined, DownOutlined} from "@ant-design/icons";
import {ButtonWithIcon} from "@/app/components/buttonWithIcon";

const {confirm} = Modal;

export type PnActsFilterType = {
    sort?: string;
    sortDir?: number;
    submitterPerson?: string;
    resourceTargetPerson?: string;
    conciliator?: string;
    resourceStatus?: string;
    documentDateFrom?: string;
    documentDateTo?: string;
    returnDateFrom?: string;
    returnDateTo?: string;
    dueDateFrom?: string;
    dueDateTo?: string;
    resourceSubTypeIds?: string;
    resourceTargetPersonTypeIds?: string;
    resourceNumber?: string;
    resourceName?: string;
    educationalInstitutionIds?: string;
    serialNumber?: string;
    inventoryNumber?: string;
    PNAStatusIds?: string[];
    returnResourceStateIds?: string[];
    resourceTargetPersonEducationalStatusIds?: string[];
    resourceTargetPersonWorkStatusIds?: string[];
    status?: string;
    resourceDiffer?: boolean;
    page: number;
    take: number;
}

export const initialValues = {
    page: 1,
    take: AppConfig.takeLimit,
};

const PnAkti = () => {
    const [modalOpen, setModalOpen] = useState(false);
    const [filter, setFilter] = useState(initialValues);
    const [selectionType, setSelectionType] = useState<'checkbox' | 'radio'>('checkbox');

    const [changeFiltersModalIsOpen, setChangeFiltersModalIsOpen] = useState<boolean>(false)
    const [changeSequenceModalIsOpen, setChangeSequenceModalIsOpen] = useState<boolean>(false);
    const [userFilterOptions, setUserFilterOptions] = useState<any[]>([]);

    const [form] = Form.useForm();
    const pageTopRef = useRef(null);

    const router = useRouter();

    const {data: sessionData} = useSession();

    const {
        data: acts,
        appendData: refetchWithUpdatedData,
        refetch,
        isLoading,
    } = useQueryApiClient({
        request: {
            url: '/applicationResources',
            data: filter,
        },
    });

    const {appendData: deleteAppendData} = useQueryApiClient({
        request: {
            url: `/parskati/:id`,
            method: 'DELETE',
        },
        onSuccess: () => {
            refetch();
        },
    });

    const {
        isLoading: filtersLoading,
        data: profileData,
        refetch: refetchUserProfile
    } = useQueryApiClient({
        request: {
            url: `/userProfile/${sessionData?.user?.profileId}`,
        },
        onSuccess: (profileData: UserProfile) => {
            if (profileData && profileData.configurationInfo) {
                const configurationInfo = JSON.parse(profileData.configurationInfo)
                if (configurationInfo.applicationResourcesColumns) {
                    const newColumns: ColumnsType<PnAct> = []
                    configurationInfo.applicationResourcesColumns.map((column: SortableTableItem) => {
                        if (column.isEnabled) {
                            const foundColumn = defaultColumns.find(c => c.key === column.key)
                            if (foundColumn) {
                                newColumns.push(foundColumn)
                            }
                        }
                    })
                    setColumns(newColumns)
                }

                if (configurationInfo.applicationResourcesFilters) {
                    const newFilters: any[] = []
                    configurationInfo.applicationResourcesFilters.map((filter: any) => {
                        if (filter.isEnabled) {
                            newFilters.push(filter)
                        }
                    })
                    setUserFilterOptions(newFilters)
                }
            }
        }
    });

    const showConfirm = (userId: string) => {
        confirm({
            title: 'Vai tiešām vēlaties dzēst šos vienumus?',
            okText: 'Dzēst',
            okType: 'danger',
            cancelText: 'Atcelt',
            async onOk() {
                deleteAppendData([], {userId});
            },
            onCancel() {
            },
        });
    };

    const fetchRecords = (page: number, pageSize: number) => {
        const newPage = page !== filter.page ? page : 1;
        const newFilter = { ...filter, page: newPage, take: pageSize };
        setFilter(newFilter);
        refetchWithUpdatedData(newFilter);
    };

    const handleEdit = (data: PnAct) => {
        form.resetFields();
    };

    const items = (record: PnAct) => {
        return {
            items: [
                {
                    key: '1',
                    label: (
                        <button type="button" onClick={() => showConfirm(record.id)}>
                            Dzēst
                        </button>
                    ),
                },
            ],
        };
    };

    const resourceTargetStatusBadge = (record: PnAct) => {
        if (record?.application.resourceTargetPersonWorkStatus?.value || record?.application.resourceTargetPersonEducationalStatus?.value) {

            let text = record?.application.resourceTargetPersonWorkStatus?.value ?? record?.application.resourceTargetPersonEducationalStatus?.value;

            if (record?.application.resourceTargetPersonWorkStatus?.code === 'working' || record?.application.resourceTargetPersonEducationalStatus?.code === 'studying') {
                return <Badge status="success" text={text} />
            } else {
                return <Badge status="error" text={text} />
            }
        }
        return null
    }

    const defaultColumns: ColumnsType<PnAct> = [
        {
            title: 'Nr.',
            dataIndex: 'pnaNumber',
            key: 'pnaNumber',
            render: (pnaNumber: string, record: PnAct) => <Link href={`/admin/pnakts/${record.id}`}>{pnaNumber}</Link>,
            sorter: true,
            className: '!font-semibold'
        },
        {
            title: 'Datums',
            dataIndex: 'documentDate',
            key: 'documentDate',
            render: (_: any, record: PnAct) => record.attachment?.documentDate && dayjs(record.attachment.documentDate).format(dateFormat),
            sorter: true
        },
        {
            title: 'Statuss',
            dataIndex: 'pnaStatus',
            key: 'pnaStatus',
            sorter: true,
            className: '!whitespace-nowrap',
            render: (pnaStatus: any) => {
                return <Badge color={pnaStatuses.find(el => el.code === pnaStatus.code)?.color} text={pnaStatus.value}/>
            }
        },
        {
            title: 'Resursa lietotājs',
            dataIndex: 'resourceUser',
            key: 'resourceUser',
            width: 200,
            render: (_: any, record: PnAct) => {
                const {
                    firstName,
                    lastName,
                    privatePersonalIdentifier
                } = record.application.resourceTargetPerson.persons[0]
                return `${firstName} ${lastName} (${privatePersonalIdentifier})`
            },
            sorter: true
        },
        {
            title: 'Resursa lietotāja loma',
            dataIndex: 'resourceTargetPersonType',
            key: 'resourceTargetPersonType',
            className: '!whitespace-nowrap',
            render: (_: any, record: PnAct) => (
                <>{record.application.resourceTargetPersonType.value}</>
            ),
            sorter: true
        },
        {
            title: 'Resurss',
            dataIndex: 'resource',
            key: 'resource',
            width: 200,
            render: (_: any, record: PnAct) => (
                <>{record.resource.resourceSubType.value}</>
            ),
            sorter: true
        },
        {
            title: 'Sērijas nr.',
            dataIndex: 'serialNumber',
            key: 'serialNumber',
            className: '!whitespace-nowrap',
            render: (_: any, record: PnAct) => record.resource.serialNumber,
            sorter: true
        },
        {
            title: 'Resursa nr.',
            dataIndex: 'resourceNumber',
            key: 'resourceNumber',
            className: '!whitespace-nowrap',
            render: (_: any, record: PnAct) => record.resource.resourceNumber,
            sorter: true
        },
        {
            title: 'P/N akta saskaņotājs',
            dataIndex: 'conciliator',
            key: 'conciliator',
            width: 150,
            render: (value: any, record: PnAct) => ``,
            sorter: true
        },
        {
            title: 'Resursa statuss',
            dataIndex: 'resourceStatus',
            key: 'resourceStatus',
            className: '!whitespace-nowrap',
            render: (_: any, record: PnAct) => {
                if (record?.application.resourceTargetPersonWorkStatus?.value || record?.application.resourceTargetPersonEducationalStatus?.value) {
                    return record?.application.resourceTargetPersonWorkStatus?.value ?? record?.application.resourceTargetPersonEducationalStatus?.value
                }
                return null
            },
            sorter: true
        },
        {
            title: 'Pieteikums',
            dataIndex: 'application',
            key: 'application',
            render: (_: any, record: PnAct) => <Link
                href={`/admin/application/${record.application.id}`}>{record.application.applicationNumber}</Link>,
            sorter: true,
            className: '!font-semibold'
        },
        {
            title: 'Resursa lietotāja klase/grupa',
            dataIndex: 'resourceTargetClassGroup',
            key: 'resourceTargetClassGroup',
            width: 150,
            render: (_: any, record: PnAct) => `${record.application.resourceTargetPersonClassGrade} ${record.application.resourceTargetPersonClassParallel}`,
        },
        {
            title: 'Resursa lietotāja statuss',
            dataIndex: 'resourceTargetStatus',
            key: 'resourceTargetStatus',
            className: '!whitespace-nowrap',
            render: (_: any, record: PnAct) => resourceTargetStatusBadge(record),
            sorter: true
        },
        {
            title: 'Inventāra numurs',
            dataIndex: 'inventoryNumber',
            key: 'inventoryNumber',
            render: (_: any, record: PnAct) => record.resource.inventoryNumber,
            sorter: true
        },
        {
            title: 'Izglītības iestāde',
            dataIndex: 'educationalInstitutionIds',
            key: 'educationalInstitutionIds',
            width: 150,
            render: (_: any, record: PnAct) => record.application.educationalInstitution?.name,
            sorter: true
        },
        {
            title: 'Vadoša iestāde',
            dataIndex: 'supervisor',
            key: 'supervisor',
            width: 250,
            render: (_: any, record: PnAct) => record.application.supervisor?.name,
            sorter: true
        },
        {
            title: 'Termiņš',
            dataIndex: 'assignedResourceReturnDate',
            key: 'assignedResourceReturnDate',
            render: (value: any) => value && dayjs(value).format(dateFormat),
            sorter: true
        },
        {
            title: 'Resursa veids',
            dataIndex: 'resourceType',
            key: 'resourceType',
            width: 150,
            render: (_: any, record: PnAct) => ''
        },
        {
            title: 'Resursa paveids',
            dataIndex: 'resourceSubTypeIds',
            key: 'resourceSubTypeIds',
            width: 150,
            render: (_: any, record: PnAct) => record.resource.resourceSubType.value,
            sorter: true
        },
        {
            title: 'Izsniegts atšķirīgs',
            dataIndex: 'receivedDiffers',
            key: 'receivedDiffers',
            width: 150,
            render: (_: any, record: PnAct) => 'Nē'
        },
        {
            title: 'Atgriešanas datums',
            dataIndex: 'returnDate',
            key: 'returnDate',
            className: '!whitespace-nowrap',
            render: (_: any, record: PnAct) => ''
        },
        {
            title: 'Atgriešanas statuss',
            dataIndex: 'returnStatus',
            key: 'returnStatus',
            className: '!whitespace-nowrap',
            render: (_: any, record: PnAct) => ''
        },
    ];

    const [columns, setColumns] = useState<ColumnsType<PnAct>>(defaultColumns);

    const handleTableChange = (
        pagination: TablePaginationConfig,
        sorter: SorterResult<PnAct>,
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

    const actionsDropdown = (
        <Dropdown menu={{
            items: [
                {
                    key: '1',
                    label: (
                        <div>Atteikt atzīmētajiem</div>
                    ),
                },
                {
                    key: '2',
                    label: (
                        <div>Dzēst atzīmētos</div>
                    ),
                },
                {
                    key: '3',
                    label: (
                        <div>Eksportēt sarakstu</div>
                    ),
                },
            ]
        }
        }
        >
            <span className='cursor-pointer'>Darbības <DownOutlined/></span>
        </Dropdown>
    )

    const rowSelection = {
        onChange: (selectedRowKeys: React.Key[], selectedRows: DataType[]) => {
            console.log(`selectedRowKeys: ${selectedRowKeys}`, 'selectedRows: ', selectedRows);
        }
    };

    return (
        <div className="flex flex-col gap-y-[10px]">
            <PnActsFilters
                filtersLoading={filtersLoading}
                activeFilters={filter}
                filterState={setFilter}
                refresh={refetchWithUpdatedData}
                userFilterOptions={userFilterOptions}
                setChangeFiltersModalIsOpen={setChangeFiltersModalIsOpen}
            />
            <div ref={pageTopRef} className="bg-white rounded-lg p-6">
                <div className='flex justify-end gap-2'>
                    <ButtonWithIcon
                        event={() => setModalOpen(true)}
                        label="Izveidot P/N aktu"
                    />
                    <Button onClick={() => setChangeSequenceModalIsOpen(true)}>
                        <ControlOutlined/>
                        Konfigurēt
                    </Button>
                    {changeFiltersModalIsOpen &&
                        <ChangeFiltersModal
                            setModalOpen={setChangeFiltersModalIsOpen}
                            profileData={profileData}
                            refetchUserProfile={refetchUserProfile}
                        />}
                    {changeSequenceModalIsOpen &&
                        <ChangeSequenceModal
                            setModalOpen={setChangeSequenceModalIsOpen}
                            profileData={profileData}
                            refetchUserProfile={refetchUserProfile}
                        />}
                </div>
                <div className='overflow-auto'>
                    <Table
                        loading={isLoading}
                        dataSource={acts?.items}
                        columns={[
                            ...columns,
                            // This needs to be stored individually, cause otherwise, it uses old state values
                            {
                                title: actionsDropdown,
                                dataIndex: 'operation',
                                key: 'operation',
                                width: '150px',
                                fixed: 'right',
                                className: '!text-[#1890FF]',
                                render: (_, record: PnAct) => {
                                    return (
                                        <Dropdown.Button
                                            onClick={() => goToUrl(`/admin/pnakts/${record.id}/edit`, router)}
                                            menu={items(record)}
                                        >
                                            Labot
                                        </Dropdown.Button>
                                    );
                                },
                            },
                        ]}
                        pagination={{
                            current: filter.page,
                            total: acts?.total,
                            defaultPageSize: filter.take,
                            pageSizeOptions: [25, 50, 75],
                            showSizeChanger: true,
                            showTotal: (total, range) => `${range[0]}-${range[1]} no ${total} ierakstiem`,
                            onChange: (page, takeLimit) => {
                                fetchRecords(page, takeLimit);
                                handleScroll(pageTopRef.current);
                            },
                        }}
                        onChange={(pagination, _, sorter) => handleTableChange(pagination, sorter as SorterResult<DataType>)}
                        rowKey={(record) => record.id}
                        scroll={{x: 'max-content'}}
                        rowSelection={{
                            type: selectionType,
                            ...rowSelection,
                        }}
                    />
                </div>
            </div>
        </div>
    );
};

export {PnAkti};
