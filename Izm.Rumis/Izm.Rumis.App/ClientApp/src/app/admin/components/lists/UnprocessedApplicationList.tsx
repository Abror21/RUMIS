'use client'

import { Application } from "@/app/types/Applications";
import useQueryApiClient from "@/app/utils/useQueryApiClient";
import { List, Typography } from "antd"
import Link from "next/link";

const {Title} = Typography

const UnprocessedApplicationList = () => {
    const filter = {
        page: 1,
        take: 10
    }
    const {
        data,
        appendData: refetchWithUpdatedData,
        isLoading,
        refetch
      } = useQueryApiClient({
        request: {
          url: '/applications',
          data: filter
        },
      });

    return (
        <div>
            <Title level={4}>Top10 neapstrādātie Pieteikumi</Title>
            <List
                loading={isLoading}
                size="small"
                bordered
                dataSource={data?.items}
                renderItem={(item: Application) => <List.Item><Link href={`/admin/application/${item.id}`}>{item.applicationNumber}</Link></List.Item>}
            />
        </div>
    )
}

export default UnprocessedApplicationList