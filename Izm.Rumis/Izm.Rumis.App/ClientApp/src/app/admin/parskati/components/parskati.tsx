'use client';

import {
    Button,
    Dropdown,
    Form,
    Modal,
    Table,
} from 'antd';
import type {ColumnsType} from 'antd/es/table';
import React, {useRef, useState} from 'react';

import {AppConfig, permissions} from '@/app/utils/AppConfig';
import useQueryApiClient from '@/app/utils/useQueryApiClient';
import {handleScroll} from '@/app/utils/utils';
import {useSession} from 'next-auth/react';
import {router} from "next/client";
import {ButtonWithIcon} from "@/app/components/buttonWithIcon";
import * as NProgress from "nprogress";
import {ControlOutlined, DownOutlined} from "@ant-design/icons";

const {confirm} = Modal;

interface DataType {
    id: string;
    title: string;
}

const Parskati = () => {
    const [id, setId] = useState<string | null>(null);
    const [selectionType, setSelectionType] = useState<'checkbox' | 'radio'>('checkbox');
    const [form] = Form.useForm();
    const pageTopRef = useRef(null);

    const initialValues = {
        page: 1,
        take: AppConfig.takeLimit,
    };

    const [filter, setFilter] = useState(initialValues);

    const {data: sessionData} = useSession();
    const userPermissions: string[] = sessionData?.user?.permissions || []

    const {
        data: parskati,
        appendData,
        refetch,
        isLoading,
    } = useQueryApiClient({
        request: {
            url: '/parskati',
            data: filter,
            disableOnMount: true
        },
    });

    const {appendData: deleteAppendData} = useQueryApiClient({
        request: {
            url: `/parskati/:userId`,
            method: 'DELETE',
        },
        onSuccess: () => {
            setId(null);
            refetch();
        },
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
        appendData(newFilter);
    };

    const handleEdit = (data: DataType) => {
        setId(data.id);
        form.resetFields();
    };

    const items = (record: DataType) => {
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

    const initialColumns = [
        {
            title: 'Nr.',
            dataIndex: 'id',
            key: 'id',
            show: true,
            className: '!font-semibold'
        },
        {
            title: 'Nosaukums',
            dataIndex: 'tile',
            key: 'tile',
            show: true
        }
    ];
    const columns: ColumnsType<DataType> = initialColumns.filter(column => column.show)

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

    const configDropdown = (
        <div style={{marginBottom: 16}}>
            <Dropdown menu={{
                items: [
                    {
                        key: '1',
                        label: (
                            <span>Izmainīt filtrus</span>
                        ),
                    },
                    {
                        key: '2',
                        label: (
                            <span>Izmainīt secību</span>
                        ),
                    }
                ]
            }
            }
            >
                <Button>
                    <ControlOutlined/>
                    Konfigurēt
                </Button>
            </Dropdown>
        </div>
    )

    const rowSelection = {
        onChange: (selectedRowKeys: React.Key[], selectedRows: DataType[]) => {
            console.log(`selectedRowKeys: ${selectedRowKeys}`, 'selectedRows: ', selectedRows);
        }
    };

    return (
        <div className="flex flex-col gap-y-[10px]">
            <div ref={pageTopRef} className="bg-white rounded-lg p-6">
                <div className="justify-end flex gap-2">
                    <ButtonWithIcon
                        event={() => {
                            NProgress.start();
                            router.push('/admin/parskati/new')}}
                        label="Izveidot"
                    />
                    {configDropdown}
                </div>
                <div className='overflow-auto'>
                    <Table
                        loading={isLoading}
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
                                render: (_: any, record: any) => {
                                    return (
                                        <Dropdown.Button
                                            onClick={() => router.push(`/admin/parskati/${record.id}`)}
                                            menu={items(record)}
                                        >
                                            Skatīt
                                        </Dropdown.Button>
                                    );
                                },
                            }
                        ]}
                        dataSource={parskati?.items}
                        pagination={{
                            current: filter.page,
                            total: parskati?.total,
                            defaultPageSize: filter.take,
                            pageSizeOptions: [25, 50, 75],
                            showSizeChanger: true,
                            showTotal: (total, range) => `${range[0]}-${range[1]} no ${total} ierakstiem`,
                            onChange: (page, takeLimit) => {
                                fetchRecords(page, takeLimit);
                                handleScroll(pageTopRef.current);
                            },
                        }}
                        rowKey={(record) => record.id}
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

export {Parskati};
