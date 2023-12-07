import { Table } from "antd";
import Link from "next/link";
import { useState } from "react";
import {SupervisorView as SupervisorViewType} from '@/app/types/Supervisor'

type UsersTabProps = {
    data: SupervisorViewType;
}

const UsersTab = ({data}: UsersTabProps) => {
    const [users, setUsers] = useState([])

    const columns = [
        {
            title: 'Lietotājs',
            dataIndex: 'name',
            key: 'name',
            render: (_: any, user: any) => <Link href="/">{user.name} {user.surname}</Link>,
            sorter: (a: any, b: any) => a.name.localeCompare(b.name),
        },
        {
            title: 'Loma',
            dataIndex: 'role',
            key: 'role',
            sorter: (a: any, b: any) => a.role.localeCompare(b.role),
        },
        {
            title: 'Statuss',
            dataIndex: 'status',
            key: 'status',
            sorter: (a: any, b: any) => a.status.localeCompare(b.status),
        },
    ]

    return (
        <div>
            <Table 
                columns={columns} 
                dataSource={users}
                pagination={false}
            />
        </div>
    )
}

export default UsersTab