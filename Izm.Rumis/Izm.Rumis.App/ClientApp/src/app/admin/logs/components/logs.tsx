'use client';

import { Button, DatePicker, Form, Input, Select, Table, TimePicker } from 'antd';
import dayjs from 'dayjs';
import { useRef, useState } from 'react';

import {
  AppConfig,
  dateFilterFormat,
  dateFormat,
  format,
} from '@/app/utils/AppConfig';
import useQueryApiClient from '@/app/utils/useQueryApiClient';
import { handleScroll } from '@/app/utils/utils';
import { Tabs } from '@/app/admin/logs/components/tabs';

const { Option } = Select;

const availableLevels: string[] = ['INFO', 'DEBUG', 'ERROR', 'WARN', 'FATAL'];
const availableRequestMethods: string[] = ['GET', 'POST', 'PUT', 'DELETE'];

const Logs = () => {
  const [form] = Form.useForm();
  const pageTopRef = useRef(null);

  const fixDate = (date: any, convertDateFormat: string) => {
    return date.format(convertDateFormat);
  };

  const initialValues = {
    date: dayjs(),
    timeFrom: dayjs('00:00', format),
    timeTo: dayjs('23:59', format),
    page: 1,
    take: AppConfig.takeLimit,
    sort: 'id',
    sortDir: 'desc',
  };

  const [filter, setFilter] = useState(() => {
    return {
      ...initialValues,
      date: fixDate(initialValues.date, dateFilterFormat),
      timeFrom: fixDate(initialValues.timeFrom, format),
      timeTo: fixDate(initialValues.timeTo, format),
    };
  });

  const {
    data: logs,
    appendData,
    isLoading,
  } = useQueryApiClient({
    request: {
      url: '/log',
      data: filter
    },
  });

  const fetchRecords = (page: number, pageSize: number) => {
    const newPage = page !== filter.page ? page : 1;
    const newFilter = { ...filter, page: newPage, take: pageSize };
    setFilter(newFilter);
    appendData(newFilter);
  };

  const onFinish = (values: any) => {
    const levels: string = values.levels ? values.levels.toString() : null;
    const requestMethods: string = values.requestMethods
      ? values.requestMethods.toString()
      : null;

    const newFilter = {
      ...filter,
      date: fixDate(values.date, dateFilterFormat),
      timeFrom: fixDate(values.timeFrom, format),
      timeTo: fixDate(values.timeTo, format),
      levels,
      requestMethods,
      message: values.message,
      logger: values.logger,
      requestUrl: values.requestUrl,
      traceId: values.traceId,
      userAgent: values.userAgent,
      thread: values.thread,
      username: values.username,
      ipAddress: values.ipAddress,
      page: 1,
    };
    setFilter(newFilter);

    appendData(newFilter);
  };

  const columns = [
    {
      title: 'ID',
      dataIndex: 'id',
      key: 'id',
    },
    {
      title: 'Lietotāja Id',
      dataIndex: 'userId',
      key: 'userId',
    },
    {
      title: 'Lietotāja profila Id',
      dataIndex: 'userProfileId',
      key: 'userProfileId',
    },
    {
      title: 'Personas Id',
      dataIndex: 'personId',
      key: 'personId',
    },
    {
      title: 'Izsekošanas Id',
      dataIndex: 'traceId',
      key: 'traceId',
    },
    {
      title: 'Sesijas Id',
      dataIndex: 'sessionId',
      key: 'sessionId',
    },
    {
      title: 'Lietotāja pārlūks',
      dataIndex: 'userAgent',
      key: 'userAgent',
    },
    {
      title: 'Pavediens',
      dataIndex: 'thread',
      key: 'thread',
    },
    {
      title: 'Datums',
      dataIndex: 'date',
      key: 'date',
    },
    {
      title: 'Tips',
      dataIndex: 'level',
      key: 'level',
    },
    {
      title: 'Notikums',
      dataIndex: 'logger',
      key: 'logger',
    },
    {
      title: 'Izņēmums',
      dataIndex: 'exception',
      key: 'exception',
    },
    {
      title: 'Paziņojums',
      dataIndex: 'message',
      key: 'message',
    },
    {
      title: 'Lietotājs',
      dataIndex: 'userName',
      key: 'userName',
    },
    {
      title: 'IP',
      dataIndex: 'ipAddress',
      key: 'ipAddress',
    },
    {
      title: 'URL',
      dataIndex: 'requestUrl',
      key: 'requestUrl',
    },
    {
      title: 'Metode',
      dataIndex: 'requestMethod',
      key: 'requestMethod',
    },
  ];

  return (
    <div>
      <div ref={pageTopRef}>
        <Tabs active='logs' />
        <Form
          form={form}
          name="logs"
          initialValues={initialValues}
          onFinish={onFinish}
          layout="vertical"
        >
          <div className="grid grid-cols-6 gap-2">
            <Form.Item name="date" label="Datums">
              <DatePicker format={dateFormat} className='w-full'/>
            </Form.Item>
            <Form.Item name="timeFrom" label="Laiks no">
              <TimePicker format={format} className='w-full'/>
            </Form.Item>
            <Form.Item name="timeTo" label="Laiks līdz">
              <TimePicker format={format} className='w-full'/>
            </Form.Item>
            <Form.Item name="levels" label="Tips">
              <Select mode="multiple" allowClear >
                {availableLevels.map((level: string) => (
                  <Option key={level} value={level} label={level}>
                    {level}
                  </Option>
                ))}
              </Select>
            </Form.Item>
            <Form.Item name="requestMethods" label="Metode">
              <Select mode="multiple" allowClear>
                {availableRequestMethods.map((level: string) => (
                  <Option key={level} value={level} label={level}>
                    {level}
                  </Option>
                ))}
              </Select>
            </Form.Item>
            <Form.Item name="message" label="Paziņojums">
                <Input />
            </Form.Item>
            <Form.Item name="logger" label="Notikums">
                <Input />
            </Form.Item>
            <Form.Item name="requestUrl" label="URL">
                <Input />
            </Form.Item>
            <Form.Item name="traceId" label="Izsekošanas Id">
                <Input />
            </Form.Item>
            <Form.Item name="userAgent" label="Lietotāja pārlūks">
                <Input />
            </Form.Item>
            <Form.Item name="thread" label="Pavediens">
                <Input />
            </Form.Item>
            <Form.Item name="username" label="Lietotājs">
                <Input />
            </Form.Item>
            <Form.Item name="ipAddress" label="IP">
                <Input />
            </Form.Item>
          </div>
          <Form.Item>
            <Button type="primary" htmlType="submit">
              Filtrēt
            </Button>
          </Form.Item>
        </Form>
        <div className='overflow-auto'>
          <Table
            scroll={{x: 'max-content'}}
            style={{wordBreak: 'break-word'}}
            loading={isLoading}
            columns={columns}
            dataSource={logs?.items}
            pagination={{
              current: filter.page,
              total: logs?.total,
              onChange: (page, takeLimit) => {
                fetchRecords(page, takeLimit);
                handleScroll(pageTopRef.current);
              },
            }}
            rowKey={(record) => record.id}
          />
        </div>
      </div>
    </div>
  );
};

export { Logs };
