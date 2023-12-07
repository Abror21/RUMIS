'use client';

import {
  Button,
  Form,
  Input,
  Table,
  Modal,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useState } from 'react';
import { useSession } from 'next-auth/react';

import useQueryApiClient from '@/app/utils/useQueryApiClient';

interface DataType {
  id: string;
  code: string;
  value: string;
}

const Parameters = () => {
  const [modalOpen, setModalOpen] = useState(false);
  const [active, setActive] = useState<DataType | null>(null);
  const [form] = Form.useForm();

  const { data: parameters, refetch, isLoading } = useQueryApiClient({
    request: {
      url: '/parameters',
    },
  });

  const { data: sessionData } = useSession();
  const userPermissions: string[] = sessionData?.user?.permissions || []
  const editPermission: boolean = userPermissions.includes('parameter.edit')

  const { appendData, isLoading: postLoader } = useQueryApiClient({
    request: {
      url: `/parameters/:id`,
      method: 'PUT',
    },
    onSuccess: () => {
      setActive(null);
      setModalOpen(false);
      form.resetFields();
      refetch();
    },
  });

  const handleEdit = (data: DataType) => {
    setActive(data);
    form.setFieldsValue(data);
    setModalOpen(true);
  };

  const initialColumns = [
    {
      title: 'Vērtība',
      dataIndex: 'value',
      key: 'value',
      show: true
    },
    {
      title: 'Kods',
      dataIndex: 'code',
      key: 'code',
      show: true
    },
    {
      title: 'Darbības',
      dataIndex: 'operation',
      key: 'operation',
      width: '150px',
      render: (_: any, record: DataType) => (
        <Button
          onClick={() => handleEdit(record)}
        >
          Labot
        </Button>
      ),
      show: editPermission
    },
  ];

  const columns: ColumnsType<DataType> =  initialColumns.filter(column => column.show)

  const handleParameter = async () => {
    await form.validateFields();
    const values = { ...form.getFieldsValue(true) }; // parse for no mutations
    if (active?.id) {
      appendData(values, { id: active.id });
    }
  };

  const handleCancel = () => {
    setModalOpen(false)
    form.resetFields()
    setActive(null)
  };

  return (
    <div>
      <Table
        loading={isLoading}
        columns={columns}
        dataSource={parameters}
        pagination={false}
        rowKey={(record) => record.id}
      />
      <Modal
        title={active && `Parametra - ${active.code} redģēšana`}
        centered
        open={modalOpen}
        onCancel={handleCancel}
        footer={[
          <Button key="back" onClick={handleCancel}>
            Atcelt
          </Button>,
          <Button
            key="submit"
            type="primary"
            loading={postLoader}
            onClick={handleParameter}
          >
            Saglabāt
          </Button>,
        ]}
      >
        <Form form={form} layout="vertical">
          <Form.Item
            label="Vērtība"
            name="value"
            rules={[{ required: true, message: 'Vērtība ir obligāta' }]}
          >
            <Input />
          </Form.Item>
          <Form.Item label="Kods" name="code">
            <Input disabled={true} />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default Parameters;

