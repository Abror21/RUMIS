'use client'

import { Button, Checkbox, Form, Tabs, TabsProps, Typography } from "antd";
import BasicDataTab from "./BasicDataTab";
import UsersTab from "./UsersTab";
import {SupervisorView as SupervisorViewType} from '@/app/types/Supervisor'
import LinkButton from "@/app/components/LinkButton";

const { Title } = Typography;

type SupervisorViewProps = {
    data: SupervisorViewType
}

const SupervisorView = ({data}: SupervisorViewProps) => {
    const [form] = Form.useForm();

    const tabsData: TabsProps['items'] = [
        {
            key: '1',
            label: 'Pamatdati',
            children: <BasicDataTab data={data}/>
        },
        {
            key: '2',
            label: 'Lietotāji',
            children: <UsersTab data={data} />
        },
    ]

    const initialValues = {
        status: data.status === 'ACTIVE' ? true : false
    }
    

    const onFinish = (values: any) => {

    }

    return (
        <div>
            <Form form={form} layout="vertical" onFinish={onFinish} initialValues={initialValues}>
                <div className="flex justify-between">
                    <Title level={4}>{`${data.name} (reģ. Nr. ${data.code})`}</Title>
                    <Form.Item name="status" valuePropName="checked">
                        <Checkbox>Aktīvs</Checkbox>
                    </Form.Item>
                </div>
                <Tabs defaultActiveKey="1" items={tabsData} />
                <div className="flex gap-2 mt-4">
                    <LinkButton href={'/admin/supervisors'}>Atcelt</LinkButton>
                    <Button htmlType="submit" type="primary">Saglabāt</Button>
                </div>
            </Form>
        </div>
    )
}

export default SupervisorView